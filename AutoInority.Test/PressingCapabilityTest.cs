using System.Collections.Generic;
using System.Reflection;

using AutoInority.Extentions;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 镇压资格谓词的武器效能闸：异想体当前防御行对武器伤害类型乘数低于全局阈值
    /// Automaton.PressingEfficacyThreshold（默认 0.5 = RESISTANCE/IMMUNE）即判不可镇压，
    /// 属性等级压制（level &gt; riskLevel）也不例外——打不动的强者进场只会白死。
    /// 抗性边界对齐 DefenseInfo.GetDefenseType（&le;0 IMMUNE、&lt;0.5 RESISTANCE、&lt;1 ENDURE）。
    /// </summary>
    [Collection("GameBridge")]
    public class PressingCapabilityTest
    {
        /// <summary>高等级（4 级，originStat=90 → CalculateStatLevel=4）桥接员工，R 型武器 grade=1。</summary>
        private static AgentModel Agent(int statValue)
        {
            var agent = GameBridge.CanWorkAgent();
            var primaryStat = GameBridge.Bare<WorkerPrimaryStat>();
            GameBridge.Set(primaryStat, "hp", statValue);
            GameBridge.Set(primaryStat, "mental", statValue);
            GameBridge.Set(primaryStat, "work", statValue);
            GameBridge.Set(primaryStat, "battle", statValue);
            GameBridge.Set(agent, "primaryStat", primaryStat);

            var weapon = GameBridge.Bare<WeaponModel>();
            var weaponInfo = GameBridge.Bare<EquipmentTypeInfo>();
            GameBridge.Set(weaponInfo, "grade", "1");
            GameBridge.Set(weaponInfo, "damageInfos", new[] { new DamageInfo(RwbpType.R, 1f) });
            GameBridge.Set(weapon, "metaInfo", weaponInfo);
            var equipment = (UnitEquipSpace)GameBridge.Get(agent, "_equipment");
            GameBridge.Set(equipment, "weapon", weapon);
            return agent;
        }

        /// <summary>低风险（ZAYIN=1 级）桥接收容物，注 riskLevel 数据行与防御行（走游戏真数据管道）。</summary>
        private static CreatureModel Creature(float resistR, string riskLevelData = "ZAYIN")
        {
            var creature = GameBridge.Bare<CreatureModel>();
            GameBridge.Set(creature, "script", GameBridge.Bare<CreatureBase>());
            GameBridge.Set(creature, "defenseId", "1");

            var metaInfo = GameBridge.Bare<CreatureTypeInfo>();
            GameBridge.Set(metaInfo, "creatureWorkType", CreatureWorkType.NORMAL);
            var defenseTable = new CreatureDefenseTable();
            defenseTable.Init(new Dictionary<string, DefenseInfo>
            {
                { "1", new DefenseInfo { R = resistR, W = 1f, B = 1f, P = 1f } },
            });
            GameBridge.Set(metaInfo, "defenseTable", defenseTable);
            var riskList = new CreatureTypeInfo.CreatureDataList { itemName = "riskLevel" };
            GameBridge.Set(riskList, "list", new List<CreatureTypeInfo.CreatureData>
            {
                new CreatureTypeInfo.CreatureData { data = riskLevelData, openLevel = 0 },
            });
            var dataTable = new CreatureTypeInfo.CreatureDataTable();
            dataTable.dictionary["riskLevel"] = riskList;
            GameBridge.Set(metaInfo, "dataTable", dataTable);
            GameBridge.Set(creature, "metaInfo", metaInfo);
            return creature;
        }

        [Theory]
        [InlineData(0f)]    // IMMUNE
        [InlineData(0.3f)]  // RESISTANCE
        public void IneffectiveWeaponBlocksEvenLevelSuppressedAgent(float resistR)
        {
            GameBridge.InjectSkillTypes();
            // 4 级 > 风险 1 级（原实现等级压制分支不看抗性直接 true），免疫/抗性武器一票否决
            Assert.False(Agent(90).IsCapableOfPressing(Creature(resistR)));
        }

        [Theory]
        [InlineData(0.8f)]  // ENDURE
        [InlineData(1.2f)]  // NONE 以上
        public void EffectiveWeaponKeepsLevelSuppressionCapable(float resistR)
        {
            GameBridge.InjectSkillTypes();
            Assert.True(Agent(90).IsCapableOfPressing(Creature(resistR)));
        }

        [Fact]
        public void LowerLevelAgentIncapableRegardlessOfResistance()
        {
            GameBridge.InjectSkillTypes();
            // 1 级 < 风险 2 级（TETH）：等级不足不变
            Assert.False(Agent(5).IsCapableOfPressing(Creature(1.5f, "TETH")));
        }

        [Fact]
        public void ThresholdIsRuntimeAdjustable()
        {
            GameBridge.InjectSkillTypes();
            var agent = Agent(90);
            var resistant = Creature(0.3f);
            Assert.False(agent.IsCapableOfPressing(resistant)); // 默认 0.5：RESISTANCE 否决

            try
            {
                // 阈值旋钮与置信度同列运行时可调（二期 Angela 自主调参的消费面）
                AutomationConfig.PressingEfficacyThreshold = 0.25f;
                Assert.True(agent.IsCapableOfPressing(resistant)); // 0.3 不再低于 0.25，资格恢复
            }
            finally
            {
                AutomationConfig.PressingEfficacyThreshold = 0.5f; // 共享单例，还原默认
            }
        }
    }
}
