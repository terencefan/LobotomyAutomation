using System.Collections.Generic;

using AutoInority.Creature;
using AutoInority.Extentions;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 粉红小分队工作面（PinkCorps.cs）：压迫(P)工作完成即扣逆卡巴拉计数
    /// （OnSkillGoalComplete L530-536），BAD 情绪超时也扣（L522-528）、设施内
    /// 每 5 人次死亡再扣（L538-573）；唯一回充=镇压其他异想体（L560-563）。
    /// 归零 → 本体出逃+已开放部门各撤出一只黑色军团（MakeBlackCorps L669-704）。
    /// workProb：洞察 0.4 / 沟通 0.5 / 压迫 0.3 / 本能 0。
    /// </summary>
    [Collection("GameBridge")]
    public class ArmyInBlackExtTest
    {
        private static CreatureModel Creature()
        {
            var creature = GameBridge.Bare<CreatureModel>();
            GameBridge.Set(creature, "script", GameBridge.Bare<PinkCorps>());
            var metaInfo = GameBridge.Bare<CreatureTypeInfo>();
            var nameList = new CreatureTypeInfo.CreatureDataList { itemName = "name" };
            GameBridge.Set(nameList, "list", new List<CreatureTypeInfo.CreatureData>
            {
                new CreatureTypeInfo.CreatureData { data = "粉红「小分队」", openLevel = 0 },
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

        [Fact]
        public void FactoryResolvesZayinBranch()
        {
            GameBridge.InjectSkillTypes();
            Assert.IsType<ArmyInBlackExt>(Creature().GetExtension());
        }

        [Fact]
        public void SkillSetsExcludeRepressionAndInstinct()
        {
            // 压迫=稳定扣计数源禁用；本能 workProb=0 派了白跑，一并禁用
            GameBridge.InjectSkillTypes();
            var sets = new ArmyInBlackExt(Creature()).SkillSets;
            Assert.Equal(2, sets.Length);
            Assert.Contains(sets, x => x.rwbpType == RwbpType.W);
            Assert.Contains(sets, x => x.rwbpType == RwbpType.B);
            Assert.DoesNotContain(sets, x => x.rwbpType == RwbpType.P);
            Assert.DoesNotContain(sets, x => x.rwbpType == RwbpType.R);
        }
    }
}
