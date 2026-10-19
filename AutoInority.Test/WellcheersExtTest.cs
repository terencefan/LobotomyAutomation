using System.Collections.Generic;

using AutoInority.Creature;
using AutoInority.Extentions;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 韦尔奇乐牌汽水：四类工作全部安全且回资源（Wellcheers.cs OnReleaseWork——
    /// 本能/洞察单回 40%/80%，沟通/压迫双回 30%/60%）；溺亡仅两触发点：工作结束时
    /// 员工 IsDead/IsPanic，或异想体 feelingState==BAD → SetDrink(willDie:true)。
    /// 故放宽 SkillSets 全四类，并加 BAD 心情停派闸。
    /// </summary>
    [Collection("GameBridge")]
    public class WellcheersExtTest
    {
        private static CreatureModel Creature(string feeling)
        {
            var creature = GameBridge.Bare<CreatureModel>();
            GameBridge.Set(creature, "script", GameBridge.Bare<Wellcheers>());
            var metaInfo = GameBridge.Bare<CreatureTypeInfo>();
            var nameList = new CreatureTypeInfo.CreatureDataList { itemName = "name" };
            GameBridge.Set(nameList, "list", new List<CreatureTypeInfo.CreatureData>
            {
                new CreatureTypeInfo.CreatureData { data = "韦尔奇乐牌汽水", openLevel = 0 },
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
            // feelingState 是 CreatureModel 公有字段（L29）
            GameBridge.Set(creature, "feelingState", System.Enum.Parse(typeof(CreatureFeelingState), feeling));
            return creature;
        }

        private static SkillTypeInfo Skill()
        {
            return new SkillTypeInfo { id = 1, name = "R", calledName = "本能" };
        }

        [Fact]
        public void FactoryResolvesZayinBranch()
        {
            GameBridge.InjectSkillTypes();
            Assert.IsType<WellcheersExt>(Creature("NONE").GetExtension());
        }

        [Fact]
        public void CanWorkWithRejectsBadFeeling()
        {
            GameBridge.InjectSkillTypes();
            var ext = new WellcheersExt(Creature("BAD"));
            Assert.False(ext.CanWorkWith(GameBridge.CanWorkAgent(), Skill(), out var message));
            Assert.NotNull(message);
        }

        [Fact]
        public void CanWorkWithPassesNormalFeeling()
        {
            GameBridge.InjectSkillTypes();
            var ext = new WellcheersExt(Creature("NORM"));
            Assert.True(ext.CanWorkWith(GameBridge.CanWorkAgent(), Skill(), out var message));
            Assert.Null(message);
        }

        [Fact]
        public void SkillSetsCoverAllFour()
        {
            GameBridge.InjectSkillTypes();
            var sets = new WellcheersExt(Creature("NONE")).SkillSets;
            Assert.Equal(4, sets.Length);
            Assert.Contains(sets, x => x.rwbpType == RwbpType.R);
            Assert.Contains(sets, x => x.rwbpType == RwbpType.W);
            Assert.Contains(sets, x => x.rwbpType == RwbpType.B);
            Assert.Contains(sets, x => x.rwbpType == RwbpType.P);
        }
    }
}
