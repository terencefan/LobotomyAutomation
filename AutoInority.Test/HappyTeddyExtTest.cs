using System.Collections.Generic;

using AutoInority.Creature;
using AutoInority.Extentions;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 快乐泰迪抱杀闸（HappyTeddy.cs）：异想体侧记 lastAgent（public 字段 L12），
    /// 进房 !invincible && agent==lastAgent → 抱杀必死（L47-58）。旧实现查员工侧
    /// GetRecentWorkedCreature 语义错位：A→泰迪→X→A 回泰迪必死而放行；
    /// 跨天（游戏 OnStageStart 重置 lastAgent）/换人后仍误拒。改读 lastAgent。
    /// </summary>
    [Collection("GameBridge")]
    public class HappyTeddyExtTest
    {
        private static HappyTeddyExt Ext(out HappyTeddy script)
        {
            script = GameBridge.Bare<HappyTeddy>();
            var creature = GameBridge.Bare<CreatureModel>();
            GameBridge.Set(creature, "script", script);
            var metaInfo = GameBridge.Bare<CreatureTypeInfo>();
            var nameList = new CreatureTypeInfo.CreatureDataList { itemName = "name" };
            GameBridge.Set(nameList, "list", new List<CreatureTypeInfo.CreatureData>
            {
                new CreatureTypeInfo.CreatureData { data = "快乐泰迪", openLevel = 0 },
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
            return new HappyTeddyExt(creature);
        }

        private static SkillTypeInfo Skill()
        {
            return new SkillTypeInfo { id = 2, name = "W", calledName = "洞察" };
        }

        [Fact]
        public void CanWorkWithRejectsLastAgent()
        {
            GameBridge.InjectSkillTypes();
            var ext = Ext(out var script);
            var agent = GameBridge.CanWorkAgent();
            GameBridge.Set(script, "lastAgent", agent);
            Assert.False(ext.CanWorkWith(agent, Skill(), out var message));
            Assert.NotNull(message);
            Assert.Contains("连续", message);
        }

        [Fact]
        public void CanWorkWithPassesOtherAgent()
        {
            // 泰迪 lastAgent 是别人：此人安全（旧实现会因员工侧 recent 误拒）
            GameBridge.InjectSkillTypes();
            var ext = Ext(out var script);
            GameBridge.Set(script, "lastAgent", GameBridge.CanWorkAgent("小蓝"));
            Assert.True(ext.CanWorkWith(GameBridge.CanWorkAgent(), Skill(), out var message));
            Assert.Null(message);
        }

        [Fact]
        public void CanWorkWithPassesFreshTeddy()
        {
            // lastAgent=null（跨天重置后）：任何人都能去
            GameBridge.InjectSkillTypes();
            var ext = Ext(out _);
            Assert.True(ext.CanWorkWith(GameBridge.CanWorkAgent(), Skill(), out var message));
            Assert.Null(message);
        }
    }
}
