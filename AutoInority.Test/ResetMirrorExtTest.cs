using System.Collections.Generic;

using AutoInority.Creature;
using AutoInority.Extentions;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 转性魔镜启用门槛（ResetMirror.cs）：四维重掷=15/15/15/15 基础上随机分配
    /// 「旧总和-60」（OnFinishWork，期望守恒的纯方差赌局）；同日第二次使用再 -20
    /// （_alreadyUseAgents 列表，每日 OnStageStart 清空）。
    /// 自动化只让无特长员工上镜：任一维 ≥65（4 级线，CalculateStatLevel 阈值
    /// 30/45/65/85）即有特长，重掷方差对其是净风险。
    /// </summary>
    [Collection("GameBridge")]
    public class ResetMirrorExtTest
    {
        private static ResetMirrorExt Ext(List<AgentModel> used = null)
        {
            var creature = GameBridge.Bare<CreatureModel>();
            var script = GameBridge.Bare<ResetMirror>();
            GameBridge.Set(script, "_alreadyUseAgents", used ?? new List<AgentModel>());
            GameBridge.Set(creature, "script", script);
            return new ResetMirrorExt(creature);
        }

        private static AgentModel Agent(int maxStat)
        {
            var agent = GameBridge.CanWorkAgent();
            var stat = GameBridge.Bare<WorkerPrimaryStat>();
            GameBridge.Set(stat, "hp", maxStat - 10);
            GameBridge.Set(stat, "mental", maxStat - 5);
            GameBridge.Set(stat, "battle", maxStat);
            GameBridge.Set(stat, "work", maxStat - 20);
            GameBridge.Set(agent, "primaryStat", stat);
            return agent;
        }

        [Fact]
        public void FactoryResolvesKitBranch()
        {
            var creature = GameBridge.Bare<CreatureModel>();
            GameBridge.Set(creature, "script", GameBridge.Bare<ResetMirror>());
            var metaInfo = GameBridge.Bare<CreatureTypeInfo>();
            // IsKit 读 metaInfo.creatureWorkType
            GameBridge.Set(metaInfo, "creatureWorkType", CreatureWorkType.KIT);
            GameBridge.Set(creature, "metaInfo", metaInfo);
            GameBridge.InjectSkillTypes();
            Assert.IsType<ResetMirrorExt>(creature.GetKitExtension());
        }

        [Fact]
        public void CanUseAcceptsMediocreAgent()
        {
            var ext = Ext();
            Assert.True(ext.CanUse(Agent(64)));
        }

        [Fact]
        public void CanUseRejectsSpecialtyAgent()
        {
            var ext = Ext();
            Assert.False(ext.CanUse(Agent(65)));
        }

        [Fact]
        public void CanUseRejectsAlreadyUsedAgent()
        {
            var agent = Agent(40);
            var ext = Ext(new List<AgentModel> { agent });
            Assert.False(ext.CanUse(agent));
        }
    }
}
