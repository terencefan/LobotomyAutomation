using System;
using System.Collections.Generic;
using System.Reflection;

using AutoInority.Creature;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// CheckSurvive 装备伤害修正层（ApplyEquipmentDamageModifiers）的离线驱动：
    /// armor 非 DeathAngel 时条件求值后跳过 OnTakeDamage；gifts 中 NamelessFetusGift
    /// 命中即走 0.95 修正；另钉四型伤害无盾基线（8 点血池遇 U(5,10)×12 轮必死）。
    /// 已知不可测：DeathAngelArmor 命中分支——其 OnTakeDamage 体内调用 Random.Range
    /// （引擎 icall），离线物理不可驱动。
    /// GameMode 注入 UNLIMIT_MODE 短路 GlobalGameManager 观察加成；工作技能恒
    /// N 型（GetWorkSuccessProb 恒 1f，绕开 workProbTable），伤害类型由
    /// metaInfo.workDamage 驱动 switch 分支。
    /// </summary>
    [Collection("GameBridge")] // GlobalGameManager._instance 全局，跨类串行
    public class CheckSurviveEquipmentTest
    {
        // StubExt 实例化会触发 BaseCreatureExt cctor 一次性捕获静态技能字段，
        // 必须先于该触碰注入技能表（GameBridgeSmokeTest 钉住的时序契约）
        static CheckSurviveEquipmentTest()
        {
            GameBridge.InjectSkillTypes();
        }

        private sealed class StubExt : BaseCreatureExt
        {
            internal StubExt(CreatureModel creature) : base(creature)
            {
            }
        }

        // id=0 → rwbpType 派生为 N（GetWorkSuccessProb 恒 1f，绕开 workProbTable）
        private static readonly SkillTypeInfo NormalWork = new SkillTypeInfo();

        private const float DeadPool = 8f;

        [Theory]
        [InlineData(RwbpType.R)]
        [InlineData(RwbpType.W)]
        [InlineData(RwbpType.B)]
        [InlineData(RwbpType.P)]
        public void BarePoolDiesAgainstBaselineDamage(RwbpType damageType)
        {
            using var ggm = UnlimitedMode();
            var ext = CreatureOf(damageType);
            var agent = Agent(DeadPool, DeadPool);
            Assert.False(ext.CheckSurvive(agent, NormalWork));
        }

        [Fact]
        public void NonDeathAngelArmorSkipsEquipmentModifier()
        {
            using var ggm = UnlimitedMode();
            var ext = CreatureOf(RwbpType.R);
            var agent = Agent(DeadPool, DeadPool);
            // armor 挂上但 script 非 DeathAngelArmor——修正层条件求值后跳过 OnTakeDamage；
            // GetBonus 管道要 armor.script → _model → metaInfo.bonus 全链可走
            var equipment = (UnitEquipSpace)GameBridge.Get(agent, "_equipment");
            var armor = GameBridge.Bare<ArmorModel>();
            var armorScript = new EquipmentScriptBase();
            GameBridge.Set(armorScript, "_model", armor);
            GameBridge.Set(armor, "script", armorScript);
            var armorMeta = GameBridge.Bare<EquipmentTypeInfo>();
            armorMeta.bonus = new EGObonusInfo();
            // defense 属性链：armor.GetDefense → script.GetDefense → model.metaInfo.defenseInfo.Copy()
            armorMeta.defenseInfo = new DefenseInfo();
            GameBridge.Set(armor, "metaInfo", armorMeta);
            GameBridge.Set(equipment, "armor", armor);
            Assert.False(ext.CheckSurvive(agent, NormalWork));
        }

        [Fact]
        public void NullEquipmentShortCircuitsModifier()
        {
            using var ggm = UnlimitedMode();
            var ext = CreatureOf(RwbpType.R);
            // CheckSurvive 全链里 workProb 管道会先于修正层炸 null equipment，
            // 早退分支只能反射直测 private ApplyEquipmentDamageModifiers
            var agent = GameBridge.Bare<AgentModel>();
            var method = typeof(BaseCreatureExt).GetMethod(
                "ApplyEquipmentDamageModifiers", BindingFlags.NonPublic | BindingFlags.Instance);
            var args = new object[] { agent, new DamageInfo(RwbpType.R, 5, 10) };
            method.Invoke(ext, args);
        }

        [Fact]
        public void NamelessFetusGiftAppliesDamageReduction()
        {
            using var ggm = UnlimitedMode();
            var ext = CreatureOf(RwbpType.R);
            var agent = Agent(DeadPool, DeadPool);
            var equipment = (UnitEquipSpace)GameBridge.Get(agent, "_equipment");
            var gifts = (UnitEGOgiftSpace)GameBridge.Get(equipment, "gifts");
            var gift = GameBridge.Bare<EGOgiftModel>();
            var script = new NamelessFetusGift();
            GameBridge.Set(script, "_model", gift);
            GameBridge.Set(gift, "script", script);
            // 工作概率管道会卷 gifts.GetBonus → script.GetBonus → model.metaInfo.bonus
            var giftMeta = GameBridge.Bare<EquipmentTypeInfo>();
            giftMeta.bonus = new EGObonusInfo();
            GameBridge.Set(gift, "metaInfo", giftMeta);
            gifts.replacedGifts.Add(gift);
            // 0.95 修正后伤害 U(4.75, 9.5) × 12 轮，8 点血池依然必死；命中分支不炸即覆盖
            Assert.False(ext.CheckSurvive(agent, NormalWork));
        }

        private static AgentModel Agent(float hp, float mental)
        {
            var agent = GameBridge.Bare<AgentModel>();
            GameBridge.Set(agent, "_bufList", new List<UnitBuf>());
            GameBridge.Set(agent, "_statBufList", new List<UnitStatBuf>());
            // AgentModel.maxHp 走 primaryStat.maxHP（默认 0 会被钳到 1，把 P 伤害缩成 1%）
            var primaryStat = new WorkerPrimaryStat();
            GameBridge.Set(primaryStat, "hp", 100);
            GameBridge.Set(agent, "primaryStat", primaryStat);
            var equipment = GameBridge.Bare<UnitEquipSpace>();
            GameBridge.Set(equipment, "gifts", new UnitEGOgiftSpace());
            GameBridge.Set(agent, "_equipment", equipment);
            GameBridge.Set(agent, "additionalDef", new DefenseInfo());
            agent.hp = hp;
            agent.mental = mental;
            GameBridge.Set(agent, "baseMaxHp", 100);
            return agent;
        }

        private static StubExt CreatureOf(RwbpType damageType)
        {
            var creature = GameBridge.Bare<CreatureModel>();
            GameBridge.Set(creature, "script", new CreatureBase());
            GameBridge.Set(creature, "_unit", GameBridge.Bare<CreatureUnit>());
            GameBridge.Set(creature, "_bufList", new List<UnitBuf>());
            GameBridge.Set(creature, "sefira", GameBridge.Bare<Sefira>());

            var metaInfo = GameBridge.Bare<CreatureTypeInfo>();
            // name property 走 dataTable 管道（未知 key 会触发 LogError 的引擎 icall，必须绕开）
            var nameList = new CreatureTypeInfo.CreatureDataList { itemName = "name" };
            GameBridge.Set(nameList, "list", new List<CreatureTypeInfo.CreatureData>
            {
                new CreatureTypeInfo.CreatureData { data = "测试体", openLevel = 0 },
            });
            var dataTable = new CreatureTypeInfo.CreatureDataTable();
            dataTable.dictionary["name"] = nameList;
            // GetRedusedWorkProbByCounter → riskLevelForce 同走 dataTable（未知 key 触发 LogError icall）
            var riskList = new CreatureTypeInfo.CreatureDataList { itemName = "riskLevel" };
            GameBridge.Set(riskList, "list", new List<CreatureTypeInfo.CreatureData>
            {
                new CreatureTypeInfo.CreatureData { data = "ZAYIN", openLevel = 0 },
            });
            dataTable.dictionary["riskLevel"] = riskList;
            GameBridge.Set(metaInfo, "dataTable", dataTable);
            metaInfo.feelingStateCubeBounds = new FeelingStateCubeBounds { upperBounds = new[] { 6, 9, 12 } };
            metaInfo.workDamage = new DamageInfo(damageType, 5, 10);
            GameBridge.Set(creature, "metaInfo", metaInfo);

            return new StubExt(creature);
        }

        private static GlobalGameManagerReset UnlimitedMode()
        {
            var ggm = GameBridge.Bare<GlobalGameManager>();
            ggm.gameMode = GameMode.UNLIMIT_MODE;
            typeof(GlobalGameManager)
                .GetField("_instance", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .SetValue(null, ggm);
            return new GlobalGameManagerReset();
        }

        private sealed class GlobalGameManagerReset : IDisposable
        {
            public void Dispose()
            {
                typeof(GlobalGameManager)
                    .GetField("_instance", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                    .SetValue(null, null);
            }
        }
    }
}
