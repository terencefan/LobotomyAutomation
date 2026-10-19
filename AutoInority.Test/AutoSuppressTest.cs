using System.Collections.Generic;

using AutoInority.Creature;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 镇压资格泛化委托：AutoSuppress = 等级线（WAW 以下）合取游戏
    /// CreatureBase.IsAutoSuppressable()——游戏源码三处 false 覆写（Baku 羊形态/
    /// Bunny/SmallBird），mod 原先只手写了 SmallBird。判据：
    /// Baku.IsAutoSuppressable = base && _state != BakuState.SHEEP（私有嵌套枚举
    /// SHEEP=0/CHICKEN=1，反射按底层 int 注入；bare 零字段即 SHEEP）；
    /// Bunny/SmallBird 恒 false。script 为 null（离线 bare）视为可镇压。
    /// </summary>
    [Collection("GameBridge")] // BaseCreatureExt 静态技能字段依赖 SkillTypeList 单例
    public class AutoSuppressTest
    {
        private sealed class PlainExt : BaseCreatureExt
        {
            internal PlainExt(CreatureModel creature) : base(creature)
            {
            }
        }

        /// <summary>metaInfo 注入 TETH 评级（riskLevelForce 属性读 dataTable["riskLevel"]
        /// 首条 data，先例 LaetitiaExtTest），script 按需注入。</summary>
        private static CreatureModel Creature(object script)
        {
            var creature = GameBridge.Bare<CreatureModel>();
            if (script != null)
            {
                GameBridge.Set(creature, "script", script);
            }
            var metaInfo = GameBridge.Bare<CreatureTypeInfo>();
            var riskList = new CreatureTypeInfo.CreatureDataList { itemName = "riskLevel" };
            GameBridge.Set(riskList, "list", new List<CreatureTypeInfo.CreatureData>
            {
                new CreatureTypeInfo.CreatureData { data = "TETH", openLevel = 0 },
            });
            var dataTable = new CreatureTypeInfo.CreatureDataTable();
            dataTable.dictionary["riskLevel"] = riskList;
            GameBridge.Set(metaInfo, "dataTable", dataTable);
            GameBridge.Set(creature, "metaInfo", metaInfo);
            return creature;
        }

        [Fact]
        public void BakuSheepNotAutoSuppressed()
        {
            GameBridge.InjectSkillTypes();
            // bare Baku 全零字段 = SHEEP（枚举首值）：羊形态不可自动镇压
            Assert.False(new PlainExt(Creature(GameBridge.Bare<Baku>())).AutoSuppress);
        }

        [Fact]
        public void BakuChickenAutoSuppressed()
        {
            GameBridge.InjectSkillTypes();
            var baku = GameBridge.Bare<Baku>();
            GameBridge.Set(baku, "_state", 1); // BakuState.CHICKEN（私有嵌套枚举，按底层 int 注）
            Assert.True(new PlainExt(Creature(baku)).AutoSuppress);
        }

        [Fact]
        public void BunnyNeverAutoSuppressed()
        {
            GameBridge.InjectSkillTypes();
            Assert.False(new PlainExt(Creature(GameBridge.Bare<Bunny>())).AutoSuppress);
        }

        [Fact]
        public void NullScriptDefaultsToSuppressed()
        {
            // 离线 bare（script 未注入）：?. 短路放行等级线判定
            GameBridge.InjectSkillTypes();
            Assert.True(new PlainExt(Creature(null)).AutoSuppress);
        }
    }
}
