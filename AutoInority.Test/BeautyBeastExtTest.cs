using System.Collections.Generic;

using AutoInority.Creature;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 美女和野兽连续压迫闸：游戏规则为压迫(P)进门时野兽入虚弱态、
    /// 已虚弱再压即被处决（BeautyBeast.cs:77-94,96-107），非 P 工作重置虚弱。
    /// mod 侧由 CenterBrain 工作记录（creature 粒度，与游戏 creature 态一致）
    /// 拒绝「上次 P + 本次 P」。本 PR 只接线文案（原拒绝占位符「Foo」泄漏），
    /// 判定逻辑不变。CenterBrain 为惰性单例，Reset 置空实例清场。
    /// </summary>
    [Collection("GameBridge")] // BaseCreatureExt 静态技能字段 + CenterBrain 单例，跨类串行
    public class BeautyBeastExtTest
    {
        private static CreatureModel Creature(long id)
        {
            var creature = GameBridge.Bare<CreatureModel>();
            GameBridge.Set(creature, "script", GameBridge.Bare<BeautyBeast>());
            var metaInfo = GameBridge.Bare<CreatureTypeInfo>();
            GameBridge.Set(metaInfo, "id", id);
            var nameList = new CreatureTypeInfo.CreatureDataList { itemName = "name" };
            GameBridge.Set(nameList, "list", new List<CreatureTypeInfo.CreatureData>
            {
                new CreatureTypeInfo.CreatureData { data = "美女和野兽", openLevel = 0 },
            });
            var dataTable = new CreatureTypeInfo.CreatureDataTable();
            dataTable.dictionary["name"] = nameList;
            GameBridge.Set(metaInfo, "dataTable", dataTable);
            GameBridge.Set(creature, "metaInfo", metaInfo);
            return creature;
        }

        private static SkillTypeInfo Skill(RwbpType type)
        {
            // rwbpType 为 get-only 派生属性（id 1-4 → RwbpType 同值），以 id 定型
            return new SkillTypeInfo { id = (int)type };
        }

        public BeautyBeastExtTest()
        {
            CenterBrain.Reset();
        }

        [Fact]
        public void ConsecutiveRepressionRejectsWithMessage()
        {
            GameBridge.InjectSkillTypes();
            var creature = Creature(100013);
            var ext = new BeautyBeastExt(creature);
            CenterBrain.AddRecord(GameBridge.CanWorkAgent("小蓝"), creature, Skill(RwbpType.P));
            Assert.False(ext.CanWorkWith(GameBridge.CanWorkAgent("小红"), Skill(RwbpType.P), out var message));
            Assert.NotNull(message);
            Assert.Contains("美女和野兽", message);
            Assert.Contains("小红", message);
            Assert.DoesNotContain("Foo", message);
        }

        [Fact]
        public void RepressionAfterOtherSkillPasses()
        {
            GameBridge.InjectSkillTypes();
            var creature = Creature(100013);
            var ext = new BeautyBeastExt(creature);
            CenterBrain.AddRecord(GameBridge.CanWorkAgent("小蓝"), creature, Skill(RwbpType.W));
            Assert.True(ext.CanWorkWith(GameBridge.CanWorkAgent("小红"), Skill(RwbpType.P), out var message));
            Assert.Null(message);
        }

        [Fact]
        public void NonRepressionAlwaysPasses()
        {
            GameBridge.InjectSkillTypes();
            var creature = Creature(100013);
            var ext = new BeautyBeastExt(creature);
            CenterBrain.AddRecord(GameBridge.CanWorkAgent("小蓝"), creature, Skill(RwbpType.P));
            Assert.True(ext.CanWorkWith(GameBridge.CanWorkAgent("小红"), Skill(RwbpType.W), out _));
        }

        [Fact]
        public void RecordIsCreatureScoped()
        {
            // 游戏 weakState 是 creature 级：另一只美和兽的 P 记录不影响本只
            GameBridge.InjectSkillTypes();
            var creature = Creature(100013);
            var another = Creature(999999);
            var ext = new BeautyBeastExt(creature);
            CenterBrain.AddRecord(GameBridge.CanWorkAgent("小蓝"), another, Skill(RwbpType.P));
            Assert.True(ext.CanWorkWith(GameBridge.CanWorkAgent("小红"), Skill(RwbpType.P), out _));
        }
    }
}
