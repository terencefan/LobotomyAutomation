using AutoInority.Creature;
using AutoInority.Extentions;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 光之锤全拒：它的「工作」是处刑技能（40006）——威胁分≥10 或紧急等级 LEVEL3 时，
    /// 进入房间的员工被 CancelWork+EquipHammer 强制变成锤战士，追杀全部出逃异想体，
    /// OnStageEnd 必死（Uncontrollable_LightsHammer.cs OnStageEnd → Die），无恢复路径。
    /// 人命换清场的决策由主管手动做出。
    /// </summary>
    [Collection("GameBridge")]
    public class LightsHammerExtTest
    {
        private static CreatureModel Creature()
        {
            var creature = GameBridge.Bare<CreatureModel>();
            GameBridge.Set(creature, "script", GameBridge.Bare<LightsHammer>());
            return creature;
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void CanWorkWithAlwaysRejects(int skillId)
        {
            var ext = new LightsHammerExt(Creature());
            Assert.False(ext.CanWorkWith(GameBridge.CanWorkAgent(), new SkillTypeInfo { id = skillId }, out var message));
            Assert.Null(message);
        }

        [Fact]
        public void FactoryResolvesZayinBranch()
        {
            var creature = Creature();
            var metaInfo = GameBridge.Bare<CreatureTypeInfo>();
            var nameList = new CreatureTypeInfo.CreatureDataList { itemName = "name" };
            GameBridge.Set(nameList, "list", new System.Collections.Generic.List<CreatureTypeInfo.CreatureData>
            {
                new CreatureTypeInfo.CreatureData { data = "光之锤", openLevel = 0 },
            });
            var riskList = new CreatureTypeInfo.CreatureDataList { itemName = "riskLevel" };
            GameBridge.Set(riskList, "list", new System.Collections.Generic.List<CreatureTypeInfo.CreatureData>
            {
                new CreatureTypeInfo.CreatureData { data = "ZAYIN", openLevel = 0 },
            });
            var dataTable = new CreatureTypeInfo.CreatureDataTable();
            dataTable.dictionary["name"] = nameList;
            dataTable.dictionary["riskLevel"] = riskList;
            GameBridge.Set(metaInfo, "dataTable", dataTable);
            GameBridge.Set(creature, "metaInfo", metaInfo);
            GameBridge.InjectSkillTypes();
            Assert.IsType<LightsHammerExt>(creature.GetExtension());
        }
    }
}
