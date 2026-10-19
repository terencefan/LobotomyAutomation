using System.Collections.Generic;
using System.Reflection;

using AutoInority.Creature;
using AutoInority.Extentions;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 避难所（Shelter，kit 300006）停线：进房无敌+每 5s 回 8HP/8SP
    /// （Shelter.cs L44-71），满 30s 后每 5s 随机清一个异想体计数器
    /// （L58-70）——出逃源头。CanStop 在 ≥28s 收手（留 2s 结算边际，
    /// 同 TheresiaExt），或持有者满血满精神提前收工；原 CanStop 恒真=秒断
    /// 零收益。锚点：CreatureModel._currentSkill + UseSkill._elapsedTime
    /// （私有，反射注，先例 TheresiaExtTest）；WorkerPrimaryStat.maxHP/maxMental
    /// 均派生自 hp/mental 字段。
    /// </summary>
    [Collection("GameBridge")] // GetKitExtension 静态缓存，跨类串行
    public class ShelterExtTest
    {
        private static ShelterExt ExtWith(float elapsed, int hp, int maxHp, int mental, int maxMental)
        {
            var kit = GameBridge.Bare<CreatureModel>();
            var skill = GameBridge.Bare<UseSkill>();
            GameBridge.Set(skill, "_elapsedTime", elapsed);
            var agent = GameBridge.CanWorkAgent("小红");
            var stat = GameBridge.Bare<WorkerPrimaryStat>();
            GameBridge.Set(stat, "hp", maxHp);
            GameBridge.Set(stat, "mental", maxMental);
            GameBridge.Set(agent, "primaryStat", stat);
            GameBridge.Set(agent, "hp", hp);
            GameBridge.Set(agent, "mental", mental);
            GameBridge.Set(agent, "_statBufList", new List<UnitStatBuf>());
            GameBridge.Set(skill, "agent", agent);
            GameBridge.Set(kit, "_currentSkill", skill);
            return new ShelterExt(kit);
        }

        private static CreatureModel Kit()
        {
            var kit = GameBridge.Bare<CreatureModel>();
            GameBridge.Set(kit, "script", GameBridge.Bare<Shelter>());
            var metaInfo = GameBridge.Bare<CreatureTypeInfo>();
            var nameList = new CreatureTypeInfo.CreatureDataList { itemName = "name" };
            GameBridge.Set(nameList, "list", new List<CreatureTypeInfo.CreatureData>
            {
                new CreatureTypeInfo.CreatureData { data = "避难所", openLevel = 0 },
            });
            var dataTable = new CreatureTypeInfo.CreatureDataTable();
            dataTable.dictionary["name"] = nameList;
            GameBridge.Set(metaInfo, "dataTable", dataTable);
            GameBridge.Set(metaInfo, "creatureWorkType", CreatureWorkType.KIT);
            GameBridge.Set(kit, "metaInfo", metaInfo);
            return kit;
        }

        [Fact]
        public void KitFactoryResolvesShelterBranch()
        {
            Assert.IsType<ShelterExt>(Kit().GetKitExtension());
        }

        [Theory]
        [InlineData(10f, false)]
        [InlineData(27.9f, false)]
        [InlineData(28f, true)]
        [InlineData(29f, true)]
        public void CanStopFollowsHoldThreshold(float elapsed, bool expected)
        {
            // 伤员持续引导：不到 28s 不收
            Assert.Equal(expected, ExtWith(elapsed, hp: 40, maxHp: 100, mental: 40, maxMental: 100).CanStop());
        }

        [Fact]
        public void FullStatusAgentStopsEarly()
        {
            // 满血满精神：引导收益归零，提前收工（避免空耗占用）
            Assert.True(ExtWith(5f, hp: 100, maxHp: 100, mental: 100, maxMental: 100).CanStop());
        }

        [Fact]
        public void HurtAgentKeepsChanneling()
        {
            Assert.False(ExtWith(5f, hp: 100, maxHp: 100, mental: 40, maxMental: 100).CanStop());
        }

        private static AgentModel Candidate(int hp, int maxHp, int mental, int maxMental)
        {
            var agent = GameBridge.CanWorkAgent("伤员");
            var stat = GameBridge.Bare<WorkerPrimaryStat>();
            GameBridge.Set(stat, "hp", maxHp);
            GameBridge.Set(stat, "mental", maxMental);
            GameBridge.Set(agent, "primaryStat", stat);
            GameBridge.Set(agent, "hp", hp);
            GameBridge.Set(agent, "mental", mental);
            GameBridge.Set(agent, "_statBufList", new List<UnitStatBuf>());
            return agent;
        }

        /// <summary>AgentManager.instance 惰性构造会级联 Init()，直接注入裸单例绕开。</summary>
        private static ShelterExt ExtWithAgents(float elapsed, params AgentModel[] agents)
        {
            var manager = GameBridge.Bare<AgentManager>();
            GameBridge.Set(manager, "agentList", new List<AgentModel>(agents));
            typeof(AgentManager).GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static)
                .SetValue(null, manager);
            var kit = GameBridge.Bare<CreatureModel>();
            return new ShelterExt(kit);
        }

        [Theory]
        [InlineData(100, 100, 100, 100, false)]
        [InlineData(40, 100, 100, 100, true)]
        [InlineData(100, 100, 40, 100, true)]
        public void CanUseGateAdmitsOnlyWounded(int hp, int maxHp, int mental, int maxMental, bool expected)
        {
            var ext = ExtWithAgents(0f);
            Assert.Equal(expected, ext.CanUse(Candidate(hp, maxHp, mental, maxMental)));
        }

        [Fact]
        public void SelectAgentPicksMostWounded()
        {
            var healthy = Candidate(100, 100, 100, 100);
            var light = Candidate(50, 100, 100, 100);
            var severe = Candidate(30, 100, 100, 100);
            var ext = ExtWithAgents(0f, healthy, light, severe);
            Assert.Same(severe, ext.SelectAgent());
        }

        [Fact]
        public void SelectAgentNullWhenAllHealthy()
        {
            var ext = ExtWithAgents(0f, Candidate(100, 100, 100, 100));
            Assert.Null(ext.SelectAgent());
        }

        [Fact]
        public void WantsDispatchNeedsFreeKitAndPatient()
        {
            var wounded = Candidate(40, 100, 100, 100);
            Assert.True(ExtWithAgents(0f, wounded).WantsDispatch());
            Assert.False(ExtWithAgents(0f, Candidate(100, 100, 100, 100)).WantsDispatch());
        }

        [Fact]
        public void WantsDispatchFalseWhenKitBusy()
        {
            // 忙线（currentSkill 非空）即不再要人：在治的出院后下一帧重新触发
            var kit = GameBridge.Bare<CreatureModel>();
            GameBridge.Set(kit, "_currentSkill", GameBridge.Bare<UseSkill>());
            Assert.False(new ShelterExt(kit).WantsDispatch());
        }
    }
}
