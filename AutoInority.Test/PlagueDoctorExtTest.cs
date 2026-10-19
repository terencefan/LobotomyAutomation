using System.Collections.Generic;
using System.Linq;

using AutoInority.Creature;
using AutoInority.Extentions;

using WhiteNightSpace;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 疫医闸门（游戏机制 PlagueDoctor.cs）：GOOD 达 cube → 亲吻祝福 ApostleLevel+1；
    /// ApostleLevel≥12（AdventLevel）→ 白夜降临——第 11 次后任何 GOOD 都会凑满。
    /// BAD 情绪超时扣计数，归零绑架随机存活员工强制亲吻。使徒（apostleData 记录
    /// instId+NameId 或佩戴礼物 400014）再派工：游戏只在 UI 层拦截
    /// （BlessedAgentWorkCheck），自动化派工路径由 CanWorkWith 自查 CheckApostle。
    /// </summary>
    [Collection("GameBridge")] // GetExtension 静态缓存 + SkillTypeList 单例，跨类串行
    public class PlagueDoctorExtTest
    {
        private static CreatureModel Creature(int blessings, long blessInstId = -1L)
        {
            var script = GameBridge.Bare<PlagueDoctor>();
            var data = Enumerable.Range(0, blessings).Select(_ =>
            {
                var apostle = GameBridge.Bare<ApostleData>();
                GameBridge.Set(apostle, "instId", blessInstId);
                return apostle;
            }).ToList();
            GameBridge.Set(script, "apostleData", data);

            var creature = GameBridge.Bare<CreatureModel>();
            GameBridge.Set(creature, "script", script);
            var metaInfo = GameBridge.Bare<CreatureTypeInfo>();
            // name property 走 dataTable 真数据管道，未知 key 触发 Debug.LogError icall（先例 LaetitiaExtTest）
            var nameList = new CreatureTypeInfo.CreatureDataList { itemName = "name" };
            GameBridge.Set(nameList, "list", new List<CreatureTypeInfo.CreatureData>
            {
                new CreatureTypeInfo.CreatureData { data = "疫医", openLevel = 0 },
            });
            var riskList = new CreatureTypeInfo.CreatureDataList { itemName = "riskLevel" };
            GameBridge.Set(riskList, "list", new List<CreatureTypeInfo.CreatureData>
            {
                new CreatureTypeInfo.CreatureData { data = "ZAYIN", openLevel = 0 },
            });
            var dataTable = new CreatureTypeInfo.CreatureDataTable();
            dataTable.dictionary["name"] = nameList;
            dataTable.dictionary["riskLevel"] = riskList;
            GameBridge.Set(metaInfo, "dataTable", dataTable);
            GameBridge.Set(creature, "metaInfo", metaInfo);
            return creature;
        }

        private static AgentModel ApostleAgent(long instId)
        {
            var agent = GameBridge.CanWorkAgent("小蓝");
            GameBridge.Set(agent, "instanceId", instId);
            // CheckApostle 走 _agentName.id 比对
            GameBridge.Set(agent, "_agentName", GameBridge.Bare<AgentName>());
            return agent;
        }

        private static SkillTypeInfo Skill()
        {
            return new SkillTypeInfo { id = 2, name = "W", calledName = "洞察" };
        }

        [Fact]
        public void FactoryResolvesZayinBranch()
        {
            // 工厂 TODO 空分支的回归钉：ZAYIN 工厂可解析出疫医 Ext
            GameBridge.InjectSkillTypes();
            Assert.IsType<PlagueDoctorExt>(Creature(0).GetExtension());
        }

        [Fact]
        public void CanWorkWithRejectsEleventhBlessing()
        {
            GameBridge.InjectSkillTypes();
            var ext = new PlagueDoctorExt(Creature(11));
            Assert.False(ext.CanWorkWith(GameBridge.CanWorkAgent(), Skill(), out var message));
            Assert.NotNull(message);
            Assert.Contains("十一", message);
        }

        [Fact]
        public void CanWorkWithPassesAtTenBlessings()
        {
            GameBridge.InjectSkillTypes();
            var ext = new PlagueDoctorExt(Creature(10));
            Assert.True(ext.CanWorkWith(GameBridge.CanWorkAgent(), Skill(), out var message));
            Assert.Null(message);
        }

        [Fact]
        public void CanWorkWithRejectsApostleAgent()
        {
            GameBridge.InjectSkillTypes();
            var ext = new PlagueDoctorExt(Creature(1, blessInstId: 7L));
            Assert.False(ext.CanWorkWith(ApostleAgent(7L), Skill(), out var message));
            Assert.NotNull(message);
            Assert.Contains("使徒", message);
        }

        [Fact]
        public void SkillSetsExcludeInstinct()
        {
            // 本能(R) 基础成功率 0.4 显著低于其余三类 0.6
            GameBridge.InjectSkillTypes();
            var sets = new PlagueDoctorExt(Creature(0)).SkillSets;
            Assert.Equal(3, sets.Length);
            Assert.Contains(sets, x => x.rwbpType == RwbpType.W);
            Assert.Contains(sets, x => x.rwbpType == RwbpType.B);
            Assert.Contains(sets, x => x.rwbpType == RwbpType.P);
            Assert.DoesNotContain(sets, x => x.rwbpType == RwbpType.R);
        }
    }
}
