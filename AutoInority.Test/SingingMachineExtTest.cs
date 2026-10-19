using System.Collections.Generic;

using AutoInority.Creature;
using AutoInority.Extentions;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 歌唱机双闸（SingingMachine.cs）：Rstat&gt;=4 || Bstat&lt;=2 完成工作即被吃
    /// （OnReleaseWork L100-113，现有闸 Rstat&gt;3||Bstat&lt;3 等价）；counter 上限 1、
    /// BAD -1（L108-111）；counter==0 时下一名进房者无条件被吃（L83-90）→
    /// ①拒派 ②只接受 GOOD 置信（BAD 即归零）。R/B 与 AgentModel.cs:161/165 对应。
    /// </summary>
    [Collection("GameBridge")]
    public class SingingMachineExtTest
    {
        private static CreatureModel Creature(int counter, bool overloaded = false)
        {
            var creature = GameBridge.Bare<CreatureModel>();
            GameBridge.Set(creature, "script", GameBridge.Bare<SingingMachine>());
            GameBridge.Set(creature, "_qliphothCounter", counter);
            GameBridge.Set(creature, "isOverloaded", overloaded);
            var metaInfo = GameBridge.Bare<CreatureTypeInfo>();
            var nameList = new CreatureTypeInfo.CreatureDataList { itemName = "name" };
            GameBridge.Set(nameList, "list", new List<CreatureTypeInfo.CreatureData>
            {
                new CreatureTypeInfo.CreatureData { data = "歌唱机", openLevel = 0 },
            });
            var riskList = new CreatureTypeInfo.CreatureDataList { itemName = "riskLevel" };
            GameBridge.Set(riskList, "list", new List<CreatureTypeInfo.CreatureData>
            {
                new CreatureTypeInfo.CreatureData { data = "HE", openLevel = 0 },
            });
            var dataTable = new CreatureTypeInfo.CreatureDataTable();
            dataTable.dictionary["name"] = nameList;
            dataTable.dictionary["riskLevel"] = riskList;
            GameBridge.Set(metaInfo, "dataTable", dataTable);
            GameBridge.Set(creature, "metaInfo", metaInfo);
            return creature;
        }

        private static AgentModel CapableAgent()
        {
            // 勇气/自律均在歌唱机安全区间（Rstat<=3, Bstat>=3）
            var agent = GameBridge.CanWorkAgent();
            GameBridge.Set(agent, "_statBufList", new List<UnitStatBuf>());
            var stat = GameBridge.Bare<WorkerPrimaryStat>();
            GameBridge.Set(stat, "hp", 50);   // fortitude → 2 级
            GameBridge.Set(stat, "mental", 50);
            GameBridge.Set(stat, "battle", 50);
            GameBridge.Set(stat, "work", 50); // temperance → 3 级
            GameBridge.Set(agent, "primaryStat", stat);
            return agent;
        }

        private static SkillTypeInfo Skill()
        {
            return new SkillTypeInfo { id = 1, name = "R", calledName = "本能" };
        }

        [Fact]
        public void FactoryResolvesHeBranch()
        {
            GameBridge.InjectSkillTypes();
            Assert.IsType<SingingMachineExt>(Creature(1).GetExtension());
        }

        [Fact]
        public void CanWorkWithRejectsEmptyCounter()
        {
            GameBridge.InjectSkillTypes();
            var ext = new SingingMachineExt(Creature(0));
            Assert.False(ext.CanWorkWith(CapableAgent(), Skill(), out var message));
            Assert.NotNull(message);
            Assert.Contains("塞进机器", message);
        }

        [Fact]
        public void CanWorkWithPassesCapableAgentAtFullCounter()
        {
            GameBridge.InjectSkillTypes();
            var ext = new SingingMachineExt(Creature(1));
            Assert.True(ext.CanWorkWith(CapableAgent(), Skill(), out var message));
            Assert.Null(message);
        }

        [Fact]
        public void CheckWorkConfidenceAcceptsOverloaded()
        {
            GameBridge.InjectSkillTypes();
            var ext = new SingingMachineExt(Creature(1, overloaded: true));
            Assert.True(ext.CheckWorkConfidence(CapableAgent(), Skill()));
        }
    }
}
