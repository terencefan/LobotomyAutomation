using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using AutoInority.Command;
using AutoInority.Posture;

using UnityEngine;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 镇压姿态撤离分级：强弱判定（等级/武器效能经 IsCapableOfPressing）、威胁部门
    /// 分桶、撤离幂等闸、安全部门选址（Graph 距离为硬编码纯表，离线可测）。
    /// 引擎耦合的撤离落地动作（SetWaitingPassage 的地图点击模拟）不入离线范围。
    /// </summary>
    [Collection("GameBridge")]
    public class RetreatCommandTest
    {
        private static void SetStatic(System.Type type, string field, object value)
        {
            var f = type.GetField(field, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(f);
            f.SetValue(null, value);
        }

        private static void InjectManagers()
        {
            var creatureManager = GameBridge.Bare<CreatureManager>();
            SetStatic(typeof(CreatureManager), "_instance", creatureManager);
            GameBridge.Set(creatureManager, "creatureList", new List<CreatureModel>());

            var ordealManager = GameBridge.Bare<OrdealManager>();
            SetStatic(typeof(OrdealManager), "_instance", ordealManager);
            GameBridge.Set(ordealManager, "ordealCreatureList", new List<OrdealCreatureModel>());

            var agentManager = GameBridge.Bare<AgentManager>();
            SetStatic(typeof(AgentManager), "_instance", agentManager);
            GameBridge.Set(agentManager, "agentList", new List<AgentModel>());

            var globalGameManager = GameBridge.Bare<GlobalGameManager>();
            SetStatic(typeof(GlobalGameManager), "_instance", globalGameManager);
            GameBridge.Set(globalGameManager, "_language", SystemLanguage.ChineseSimplified);
        }

        private static void InjectSefiraManager(params Sefira[] sefiras)
        {
            var manager = GameBridge.Bare<SefiraManager>();
            SetStatic(typeof(SefiraManager), "_instance", manager);
            GameBridge.Set(manager, "sefiraList", new List<Sefira>(sefiras));
            var dic = new Dictionary<SefiraEnum, Sefira>();
            foreach (var sefira in sefiras)
            {
                dic[(SefiraEnum)GameBridge.Get(sefira, "_sefiraEnum")] = sefira;
            }
            GameBridge.Set(manager, "_sefiraDic", dic);
        }

        private static Sefira BridgedSefira(SefiraEnum sefiraEnum, bool activated = true)
        {
            var sefira = GameBridge.Bare<Sefira>();
            GameBridge.Set(sefira, "_sefiraEnum", sefiraEnum);
            GameBridge.Set(sefira, "_activated", activated);
            GameBridge.Set(sefira, "passageList", new List<PassageObjectModel>());
            return sefira;
        }

        private static PassageObjectModel BridgedPassage(SefiraEnum sefiraEnum, bool isActivate, PassageType type)
        {
            var passage = GameBridge.Bare<PassageObjectModel>();
            GameBridge.Set(passage, "sefiraEnum", sefiraEnum);
            GameBridge.Set(passage, "isActivate", isActivate);
            GameBridge.Set(passage, "type", type);
            return passage;
        }

        /// <summary>低风险（ZAYIN）出逃桥接收容物，可注防御行与收容部门。</summary>
        private static CreatureModel EscapedCreature(Sefira containment, float resistR = 1f)
        {
            var creature = GameBridge.Bare<CreatureModel>();
            GameBridge.Set(creature, "script", GameBridge.Bare<CreatureBase>());
            GameBridge.Set(creature, "isOverloaded", false);
            GameBridge.Set(creature, "_state", CreatureState.ESCAPE);
            GameBridge.Set(creature, "sefira", containment);
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
                new CreatureTypeInfo.CreatureData { data = "ZAYIN", openLevel = 0 },
            });
            var dataTable = new CreatureTypeInfo.CreatureDataTable();
            dataTable.dictionary["riskLevel"] = riskList;
            GameBridge.Set(metaInfo, "dataTable", dataTable);
            GameBridge.Set(creature, "metaInfo", metaInfo);

            var unit = GameBridge.Bare<CreatureUnit>();
            GameBridge.Set(unit, "room", GameBridge.Bare<IsolateRoom>());
            GameBridge.Set(creature, "_unit", unit);
            return creature;
        }

        /// <summary>IDLE 桥接员工：statLevel 经 CalculateStatLevel 阈值映射（5/40/50/90/100 → 1..5 级）。</summary>
        private static AgentModel BridgedAgent(SefiraEnum sefira, int statLevel = 1)
        {
            var agent = GameBridge.CanWorkAgent();
            GameBridge.Set(agent, "_state", AgentAIState.IDLE);
            GameBridge.Set(agent, "currentSefiraEnum", sefira);

            var stat = new[] { 5, 40, 50, 90, 100 }[statLevel - 1];
            var primaryStat = GameBridge.Bare<WorkerPrimaryStat>();
            GameBridge.Set(primaryStat, "hp", stat);
            GameBridge.Set(primaryStat, "mental", stat);
            GameBridge.Set(primaryStat, "work", stat);
            GameBridge.Set(primaryStat, "battle", stat);
            GameBridge.Set(agent, "primaryStat", primaryStat);
            // titleBonus 是计算属性（prefix 为 null 即零加成），无需注入

            // 武器 grade=1（同级判定分支走防御行），伤害类型 R
            var weapon = GameBridge.Bare<WeaponModel>();
            var weaponInfo = GameBridge.Bare<EquipmentTypeInfo>();
            GameBridge.Set(weaponInfo, "grade", "1");
            GameBridge.Set(weaponInfo, "damageInfos", new[] { new DamageInfo(RwbpType.R, 1f) });
            GameBridge.Set(weapon, "metaInfo", weaponInfo);
            var equipment = (UnitEquipSpace)GameBridge.Get(agent, "_equipment");
            GameBridge.Set(equipment, "weapon", weapon);
            return agent;
        }

        private static List<ICommand> QueueData()
        {
            var queue = (PriorityQueue<ICommand>)GameBridge.Get(Automaton.Instance, "_commandQueue");
            return GameBridge.Get(queue, "data") as List<ICommand>;
        }

        [Fact]
        public void ScoreSafetyIsMinGraphDistanceFromThreats()
        {
            // Graph._sefiraDistance 硬编码表：威胁 TIPERERTH1(4) 时 MALKUT(0) 距 2、CHESED(7) 距 1
            var threats = new HashSet<SefiraEnum> { SefiraEnum.TIPERERTH1 };
            Assert.Equal(2, RetreatCommand.ScoreSafety(BridgedSefira(SefiraEnum.MALKUT), threats));
            Assert.Equal(1, RetreatCommand.ScoreSafety(BridgedSefira(SefiraEnum.CHESED), threats));
            Assert.Equal(0, RetreatCommand.ScoreSafety(BridgedSefira(SefiraEnum.MALKUT), new HashSet<SefiraEnum>()));
        }

        [Fact]
        public void FindSafePassagePicksFarthestOpenedHorizontal()
        {
            InjectManagers();
            var malkut = BridgedSefira(SefiraEnum.MALKUT);
            var chesed = BridgedSefira(SefiraEnum.CHESED);
            var binah = BridgedSefira(SefiraEnum.BINAH, activated: false);
            var horizontal = BridgedPassage(SefiraEnum.MALKUT, isActivate: true, type: PassageType.HORIZONTAL);
            GameBridge.Set(malkut, "passageList", new List<PassageObjectModel>
            {
                BridgedPassage(SefiraEnum.MALKUT, isActivate: false, type: PassageType.HORIZONTAL),
                BridgedPassage(SefiraEnum.MALKUT, isActivate: true, type: PassageType.VERTICAL),
                horizontal,
            });
            GameBridge.Set(chesed, "passageList", new List<PassageObjectModel>
            {
                BridgedPassage(SefiraEnum.CHESED, isActivate: true, type: PassageType.HORIZONTAL),
            });
            GameBridge.Set(binah, "passageList", new List<PassageObjectModel>
            {
                BridgedPassage(SefiraEnum.BINAH, isActivate: true, type: PassageType.HORIZONTAL),
            });
            InjectSefiraManager(malkut, chesed, binah);

            // 威胁 = TIPERERTH1（出逃收容物所在部门）：MALKUT 距 2 > CHESED 距 1，BINAH 未开启
            var creatureManager = (CreatureManager)typeof(CreatureManager)
                .GetField("_instance", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .GetValue(null);
            GameBridge.Set(creatureManager, "creatureList", new List<CreatureModel> { EscapedCreature(BridgedSefira(SefiraEnum.TIPERERTH1)) });

            Assert.Same(horizontal, RetreatCommand.FindSafePassage());
        }

        [Fact]
        public void SefiraOfFallsBackToContainmentWithoutNode()
        {
            Assert.Equal(SefiraEnum.MALKUT, RetreatCommand.SefiraOf(EscapedCreature(BridgedSefira(SefiraEnum.MALKUT))));
        }

        [Fact]
        public void EnqueueRetreatsOnlyWeakIdlesInThreatenedDepartments()
        {
            InjectManagers();
            InjectSefiraManager(BridgedSefira(SefiraEnum.MALKUT), BridgedSefira(SefiraEnum.YESOD));
            QueueData().Clear();
            while (Automaton.Instance.CurrentPostureEnum != PostureEnum.SUPPRESS)
            {
                Automaton.Instance.TogglePosture();
            }

            var malkut = BridgedSefira(SefiraEnum.MALKUT);
            var creatureManager = (CreatureManager)typeof(CreatureManager)
                .GetField("_instance", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .GetValue(null);
            GameBridge.Set(creatureManager, "creatureList", new List<CreatureModel> { EscapedCreature(malkut) });

            var agentManager = (AgentManager)typeof(AgentManager)
                .GetField("_instance", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .GetValue(null);
            var strong = BridgedAgent(SefiraEnum.MALKUT, statLevel: 4);   // 4 级 > 风险 1：本地强者留下
            var weak = BridgedAgent(SefiraEnum.MALKUT, statLevel: 1);     // 1 级同级不过防御行：撤离
            var safe = BridgedAgent(SefiraEnum.YESOD, statLevel: 1);      // 安全部门不撤
            GameBridge.Set(agentManager, "agentList", new List<AgentModel> { strong, weak, safe });

            Automaton.Instance.CurrentPosture.OnTick();
            var queue = QueueData();
            Assert.Single(queue.Where(x => x is SuppressCommand));
            var retreats = queue.OfType<RetreatCommand>().ToList();
            Assert.Single(retreats);
        }

        [Fact]
        public void RetreatCommandIsApplicableIdempotentGate()
        {
            InjectManagers();
            InjectSefiraManager(BridgedSefira(SefiraEnum.MALKUT), BridgedSefira(SefiraEnum.YESOD));
            QueueData().Clear();
            while (Automaton.Instance.CurrentPostureEnum != PostureEnum.SUPPRESS)
            {
                Automaton.Instance.TogglePosture();
            }

            var creatureManager = (CreatureManager)typeof(CreatureManager)
                .GetField("_instance", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .GetValue(null);
            GameBridge.Set(creatureManager, "creatureList", new List<CreatureModel> { EscapedCreature(BridgedSefira(SefiraEnum.MALKUT)) });

            var agent = BridgedAgent(SefiraEnum.MALKUT, statLevel: 1);
            var command = new RetreatCommand(agent);
            Assert.True(command.IsApplicable); // 无等待通道：需撤离

            GameBridge.Set(agent, "_currentWaitingPassage", BridgedPassage(SefiraEnum.YESOD, isActivate: true, type: PassageType.HORIZONTAL));
            Assert.False(command.IsApplicable); // 已在撤往安全部门的路上：幂等闸

            GameBridge.Set(agent, "_currentWaitingPassage", BridgedPassage(SefiraEnum.MALKUT, isActivate: true, type: PassageType.HORIZONTAL));
            Assert.True(command.IsApplicable); // 通道仍在威胁部门：继续撤

            GameBridge.Set(agent, "_currentWaitingPassage", null);
            GameBridge.Set(agent, "_state", AgentAIState.SUPPRESS_CREATURE);
            Assert.False(command.IsApplicable); // 镇压中不打断（战中撤退归 AgentOnFixedUpdate）
        }
    }
}
