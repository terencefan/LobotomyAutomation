using System.Collections.Generic;

using AutoInority.Creature;
using AutoInority.Extentions;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 银河之子（T-09-87 星星之子：GalaxyBoy）：counter 上限 5；完工时 token 持有者
    /// +1、无 token 者成为新持有者（GalaxyBoy.cs L80-107）；counter==0 时任何工作
    /// 完工先 +1 并发新 token（L92-95）——自救路径。FriendTokenBuf：持有者在别处
    /// 工作受 counter×4 伤害且 -1，故 BlessedCount&gt;=3 的无 token 职员被拒。
    /// 本次放宽：counter==0 时放行无 token 职员，解除受福者全灭后（无 token 可传）
    /// 永久闲置的软死锁。IsUrgent（counter&lt;5）不变。
    /// </summary>
    [Collection("GameBridge")] // GetExtension 静态缓存 + CenterBrain 单例，跨类串行
    public class GalaxyBoyExtTest
    {
        private static CreatureModel Creature(int counter)
        {
            var creature = GameBridge.Bare<CreatureModel>();
            GameBridge.Set(creature, "script", GameBridge.Bare<GalaxyBoy>());
            GameBridge.Set(creature, "_qliphothCounter", counter);
            var metaInfo = GameBridge.Bare<CreatureTypeInfo>();
            // name property 走 dataTable 真数据管道，未知 key 触发 Debug.LogError icall（先例 LadyLookingAtWallTest）
            var nameList = new CreatureTypeInfo.CreatureDataList { itemName = "name" };
            GameBridge.Set(nameList, "list", new List<CreatureTypeInfo.CreatureData>
            {
                new CreatureTypeInfo.CreatureData { data = "星星之子", openLevel = 0 },
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

        private static AgentModel Agent()
        {
            return GameBridge.CanWorkAgent("小红");
        }

        private static AgentModel TokenAgent()
        {
            var agent = GameBridge.CanWorkAgent("小红");
            var buf = GameBridge.Bare<UnitBuf>();
            GameBridge.Set(buf, "type", UnitBufType.FRIEND_TOKEN);
            // HasUnitBuf 走 _bufList（先例 ShyLookDoorTest 的心之归属地检测）
            GameBridge.Set(agent, "_bufList", new List<UnitBuf> { buf });
            return agent;
        }

        private static SkillTypeInfo Skill()
        {
            return new SkillTypeInfo { id = 1, name = "R", calledName = "本能" };
        }

        public GalaxyBoyExtTest()
        {
            // CenterBrain 单例跨测试残留，先例 BeautyBeastExtTest 构造器 Reset
            CenterBrain.Reset();
        }

        [Fact]
        public void FactoryResolvesHeBranch()
        {
            Assert.IsType<GalaxyBoyExt>(Creature(5).GetExtension());
        }

        [Fact]
        public void TokenHolderAlwaysPasses()
        {
            CenterBrain.Reset();
            var ext = new GalaxyBoyExt(Creature(2));
            Assert.True(ext.CheckWorkConfidence(TokenAgent(), Skill()));
        }

        [Fact]
        public void TokenlessAtMaxBlessingRejected()
        {
            // BlessedCount=3：无 token 职员工作会成为第四受福者/接棒受伤，拒
            var creature = Creature(2);
            CenterBrain.AddRecord(GameBridge.CanWorkAgent("小蓝"), creature, Skill());
            CenterBrain.AddRecord(GameBridge.CanWorkAgent("小黄"), creature, Skill());
            CenterBrain.AddRecord(GameBridge.CanWorkAgent("小绿"), creature, Skill());
            var ext = new GalaxyBoyExt(creature);
            Assert.False(ext.CheckWorkConfidence(Agent(), Skill()));
        }

        [Fact]
        public void CounterZeroSelfRescuePasses()
        {
            // counter==0：任何工作完工先 +1 发新 token（自救），放行 —— 修复钉：
            // 旧逻辑在受福者全灭后（BlessedCount=3 且场上无 token）永久拒绝
            var creature = Creature(0);
            CenterBrain.AddRecord(GameBridge.CanWorkAgent("小蓝"), creature, Skill());
            CenterBrain.AddRecord(GameBridge.CanWorkAgent("小黄"), creature, Skill());
            CenterBrain.AddRecord(GameBridge.CanWorkAgent("小绿"), creature, Skill());
            var ext = new GalaxyBoyExt(creature);
            Assert.True(ext.CheckWorkConfidence(Agent(), Skill()));
        }

        [Fact]
        public void TokenlessUnderMaxBlessingPasses()
        {
            // BlessedCount=2：无 token 职员本就放行（接棒发 token 路径）
            var creature = Creature(2);
            CenterBrain.AddRecord(GameBridge.CanWorkAgent("小蓝"), creature, Skill());
            CenterBrain.AddRecord(GameBridge.CanWorkAgent("小黄"), creature, Skill());
            var ext = new GalaxyBoyExt(creature);
            Assert.True(ext.CheckWorkConfidence(Agent(), Skill()));
        }
    }
}
