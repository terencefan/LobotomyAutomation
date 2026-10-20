using System.Collections.Generic;
using System.Reflection;

using AutoInority.Command;
using AutoInority.Extentions;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 训练命令的门检语义：紧急熄火（IsApplicable 随 InEmergency 翻转）、
    /// 完成随可用性翻转（IsCompleted）、守卫分支（满级直接拒绝、无候选空手而归）
    /// 与命令等价性。成功路径（图链选房 + Apply 落工作）依赖 MapGraph/
    /// SefiraMapLayer 引擎图结构，离线不可驱动，不在此钉。
    /// </summary>
    [Collection("GameBridge")]
    public class TrainCommandTest
    {
        static TrainCommandTest()
        {
            // MaxStat 链读 MissionManager（私有构造不初始化任务列表），注入 bare + 空表
            var missionManager = GameBridge.Bare<MissionManager>();
            typeof(MissionManager)
                .GetField("_instance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .SetValue(null, missionManager);
            GameBridge.Set(missionManager, "clearedMissions", new List<Mission>());
            GameBridge.Set(missionManager, "closedMissions", new List<Mission>());
        }

        private static AgentModel TrainableAgent()
        {
            var agent = GameBridge.CanWorkAgent();
            GameBridge.Set(agent, "primaryStat", new WorkerPrimaryStat { hp = 10, mental = 10, work = 10, battle = 10 });
            GameBridge.Set(agent, "primaryStatExp", new WorkerPrimaryStatExp());
            GameBridge.Set(agent, "_statBufList", new List<UnitStatBuf>());
            agent.hp = agent.maxHp;
            agent.mental = agent.maxMental;
            return agent;
        }

        private static AgentModel MaxedAgent()
        {
            var agent = GameBridge.CanWorkAgent();
            GameBridge.Set(agent, "primaryStat", new WorkerPrimaryStat { hp = 130, mental = 130, work = 130, battle = 130 });
            GameBridge.Set(agent, "primaryStatExp", new WorkerPrimaryStatExp());
            GameBridge.Set(agent, "_statBufList", new List<UnitStatBuf>());
            agent.hp = agent.maxHp;
            agent.mental = agent.maxMental;
            return agent;
        }

        [Fact]
        public void ApplicabilityFlipsWithEmergency()
        {
            PostureScheduleTest.InjectManagers(); // 空场：无考验、无过载 → 非紧急
            var command = new TrainCommand(TrainableAgent());
            Assert.True(command.IsApplicable);

            // 塞进一个过载收容物 → InEmergency → 训练熄火
            var creatureManager = (CreatureManager)typeof(CreatureManager)
                .GetField("_instance", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .GetValue(null);
            GameBridge.Set(creatureManager, "creatureList", new List<CreatureModel>
            {
                PostureScheduleTest.BridgedCreature(CreatureState.WAIT, overloaded: true),
            });
            Assert.True(Automaton.InEmergency);
            Assert.False(command.IsApplicable);
        }

        [Fact]
        public void CompletionTracksAgentAvailability()
        {
            var agent = TrainableAgent();
            var command = new TrainCommand(agent);
            Assert.False(command.IsCompleted);

            agent.hp = 0; // 重伤退出候选池 → 命令即视为完成，等姿态下 tick 重扫
            Assert.True(command.IsCompleted);
        }

        [Fact]
        public void ExecuteRejectsMaxedAgent()
        {
            var command = new TrainCommand(MaxedAgent());
            Assert.False(command.Execute()); // 四维全满，守卫直接拒绝
        }

        [Fact]
        public void ExecuteReturnsFalseWithoutCandidates()
        {
            PostureScheduleTest.InjectManagers(); // 空收容列表：无人可选
            var command = new TrainCommand(TrainableAgent());
            Assert.False(command.Execute());
        }

        [Fact]
        public void EqualityPriorityAndHash()
        {
            var agent = TrainableAgent();
            // 只钉同员工等价（队列去重语义）。跨员工不等价无法离线钉：bare 对象
            // 跳过构造，UnityEngine.Object.Equals 按 InstanceID 判等，全部视为同一
            Assert.Equal(new TrainCommand(agent), new TrainCommand(agent));
            Assert.Equal(PriorityEnum.TRAIN_AGENT, new TrainCommand(agent).Priority());
            Assert.NotEqual(0, new TrainCommand(agent).GetHashCode());
        }
    }
}
