using System.Collections.Generic;

using AutoInority.Creature;
using AutoInority.Extentions;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 面壁女（O-01-12 老妇人邻位：LadyLookingAtWall，游戏数据 riskLevel=1 ZAYIN）
    /// 激活与闸门：原工厂分支挂在 BuildTethExt 且 BuildZayinExt 无此分支——永不达的
    /// 死代码，激活 = 分支迁入 ZAYIN 工厂 + 沟通(B)排除 + 自律闸。
    /// 游戏机制（LadyLookingAtWall.cs）：OnEnterRoom skill id==3 → SubQliphothCounter
    /// （沟通进门扣点）；OnFinishWork temperanceLevel&lt;2 → 冻结+强制恐慌；计数器
    /// NORM 50%/BAD 70% 扣（由 ExpectGoodAndNormalExt 的 counter==1 GOOD 闸覆盖）。
    /// temperanceLevel 链 = CalculateStatLevel(primaryStat.work)，阈值 30/45/65/85
    /// （&lt;30 → 1 级，50 → 3 级）。
    /// </summary>
    [Collection("GameBridge")] // GetExtension 静态缓存 + SkillTypeList 单例，跨类串行
    public class LadyLookingAtWallTest
    {
        private static CreatureModel Creature()
        {
            var creature = GameBridge.Bare<CreatureModel>();
            GameBridge.Set(creature, "script", GameBridge.Bare<LadyLookingAtWall>());
            var metaInfo = GameBridge.Bare<CreatureTypeInfo>();
            // name property 走 dataTable 真数据管道，未知 key 触发 Debug.LogError icall（先例 LaetitiaExtTest）
            var nameList = new CreatureTypeInfo.CreatureDataList { itemName = "name" };
            GameBridge.Set(nameList, "list", new List<CreatureTypeInfo.CreatureData>
            {
                new CreatureTypeInfo.CreatureData { data = "面壁女", openLevel = 0 },
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

        private static AgentModel Agent(int temperanceStat)
        {
            var agent = GameBridge.CanWorkAgent("小红");
            var stat = GameBridge.Bare<WorkerPrimaryStat>();
            GameBridge.Set(stat, "work", temperanceStat);
            GameBridge.Set(agent, "primaryStat", stat);
            // temperanceStat 链：GetPrimaryStatBuf 遍历 _statBufList（独立于 _bufList）
            GameBridge.Set(agent, "_statBufList", new List<UnitStatBuf>());
            return agent;
        }

        private static SkillTypeInfo Skill()
        {
            return new SkillTypeInfo { id = 1, name = "R", calledName = "本能" };
        }

        [Fact]
        public void FactoryResolvesZayinBranch()
        {
            // 死分支修复的回归钉：ZAYIN 工厂可解析出面壁女 Ext
            GameBridge.InjectSkillTypes();
            Assert.IsType<LadyLookingAtWallExt>(Creature().GetExtension());
        }

        [Fact]
        public void CanWorkWithRejectsLowTemperance()
        {
            GameBridge.InjectSkillTypes();
            var ext = new LadyLookingAtWallExt(Creature());
            Assert.False(ext.CanWorkWith(Agent(10), Skill(), out var message));
            Assert.NotNull(message);
            Assert.Contains("夺走心智", message);
            Assert.Contains("小红", message);
        }

        [Fact]
        public void CanWorkWithPassesCapableAgent()
        {
            GameBridge.InjectSkillTypes();
            var ext = new LadyLookingAtWallExt(Creature());
            Assert.True(ext.CanWorkWith(Agent(50), Skill(), out var message));
            Assert.Null(message);
        }

        [Fact]
        public void SkillSetsExcludeAttachment()
        {
            // 沟通(B)进门即扣计数，SkillSets 唯一排除项
            GameBridge.InjectSkillTypes();
            var sets = new LadyLookingAtWallExt(Creature()).SkillSets;
            Assert.Equal(3, sets.Length);
            Assert.Contains(sets, x => x.rwbpType == RwbpType.R);
            Assert.Contains(sets, x => x.rwbpType == RwbpType.W);
            Assert.Contains(sets, x => x.rwbpType == RwbpType.P);
            Assert.DoesNotContain(sets, x => x.rwbpType == RwbpType.B);
        }
    }
}
