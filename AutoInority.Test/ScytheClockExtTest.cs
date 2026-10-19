using System.Collections.Generic;

using AutoInority.Creature;
using AutoInority.Extentions;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 沉默的代价（v1.1，ScytheClock.cs）：无计数器、无属性死亡条件、四类工作皆可；
    /// 唯一风险=BAD 心情 → 玩家手动暂停（仅 STOPGAME）时 KillOnce 随机处刑一名存活
    /// 员工（L309-340），_killCount 跨天持久化（L71-98），XIII 后每 1.75s 杀一人至 25
    /// （L100-132）。此前无工厂分支走默认扩展（CheckWorkConfidence=true），现只接受
    /// GOOD 置信。
    /// </summary>
    [Collection("GameBridge")]
    public class ScytheClockExtTest
    {
        private static CreatureModel Creature(bool overloaded)
        {
            var creature = GameBridge.Bare<CreatureModel>();
            GameBridge.Set(creature, "script", GameBridge.Bare<ScytheClock>());
            GameBridge.Set(creature, "isOverloaded", overloaded);
            var metaInfo = GameBridge.Bare<CreatureTypeInfo>();
            var nameList = new CreatureTypeInfo.CreatureDataList { itemName = "name" };
            GameBridge.Set(nameList, "list", new List<CreatureTypeInfo.CreatureData>
            {
                new CreatureTypeInfo.CreatureData { data = "沉默的代价", openLevel = 0 },
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

        [Fact]
        public void FactoryResolvesHeBranch()
        {
            GameBridge.InjectSkillTypes();
            Assert.IsType<ScytheClockExt>(Creature(false).GetExtension());
        }

        [Fact]
        public void CheckWorkConfidenceAcceptsOverloaded()
        {
            GameBridge.InjectSkillTypes();
            var ext = new ScytheClockExt(Creature(overloaded: true));
            Assert.True(ext.CheckWorkConfidence(GameBridge.CanWorkAgent(), new SkillTypeInfo { id = 1 }));
        }
    }
}
