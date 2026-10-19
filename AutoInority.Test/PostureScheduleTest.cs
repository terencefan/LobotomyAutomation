using System.Collections.Generic;
using System.Reflection;

using AutoInority.Command;
using AutoInority.Posture;

using UnityEngine;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 姿态调度骨架：B 键切换状态机、两姿态 OnTick 的 seed 语义、Main 排空不回炉。
    /// 桥接注入三个管理器单例（Bare + 列表字段）与桥接收容物（urgent/escape），
    /// Automaton.Instance 为跨用例共享单例（Collection 串行，队列/集合状态每用例自清）。
    /// </summary>
    [Collection("GameBridge")]
    public class PostureScheduleTest
    {
        private static void SetStatic(System.Type type, string field, object value)
        {
            var f = type.GetField(field, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(f);
            f.SetValue(null, value);
        }

        /// <summary>注入空管理器单例：CreatureManager/OrdealManager/AgentManager 均为 Bare + 空列表；
        /// GlobalGameManager 供 Angela.Language 三语解析；AngelaConversationUI 为测试替身
        /// （真身 AddAngelaMessage 体内 icall 无法离线 JIT，见 Bridges/AngelaConversationUI.cs），
        /// 气泡 no-op，Announce 的 Log 半边照发走 Notice 管道。</summary>
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

            AngelaConversationUI.instance = new AngelaConversationUI();
        }

        /// <summary>
        /// 桥接收容物：script=Bare 走 BuildExtension 兜底（ExpectGoodAndNormalExt），
        /// metaInfo 注 creatureWorkType 与 riskLevel 数据行（走游戏真数据管道），
        /// state/<feelingState> 为自动属性BackingField、_unit→room 链供 IsAvailable 反射。
        /// </summary>
        private static CreatureModel BridgedCreature(CreatureState state, bool overloaded)
        {
            var creature = GameBridge.Bare<CreatureModel>();
            GameBridge.Set(creature, "script", GameBridge.Bare<CreatureBase>());
            GameBridge.Set(creature, "isOverloaded", overloaded);
            GameBridge.Set(creature, "_state", state);

            var metaInfo = GameBridge.Bare<CreatureTypeInfo>();
            GameBridge.Set(metaInfo, "creatureWorkType", CreatureWorkType.NORMAL);
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

        private static void ForcePosture(PostureEnum target)
        {
            while (Automaton.Instance.CurrentPostureEnum != target)
            {
                Automaton.Instance.TogglePosture();
            }
        }

        private static int QueueCount()
        {
            var queue = (PriorityQueue<ICommand>)GameBridge.Get(Automaton.Instance, "_commandQueue");
            var data = GameBridge.Get(queue, "data") as List<ICommand>;
            return data.Count;
        }

        /// <summary>清空共享命令队列：用例顺序不保证，队列计数断言须自给自足。</summary>
        private static void DrainQueue()
        {
            var queue = (PriorityQueue<ICommand>)GameBridge.Get(Automaton.Instance, "_commandQueue");
            var data = GameBridge.Get(queue, "data") as List<ICommand>;
            data.Clear();
        }

        private static void ResetProductionState()
        {
            Automaton.Instance.WorkingAgents.Clear();
            Automaton.Instance.FarmingCreatures.Clear();
        }

        private static bool SkipPrefix() => false;

        [Fact]
        public void TogglePostureFlipsEnumAndKeepsState()
        {
            InjectManagers();
            var agent = GameBridge.CanWorkAgent();
            var creature = BridgedCreature(CreatureState.WAIT, overloaded: false);
            var macro = new RepeatWorkCommand(agent);
            macro.Update(creature, new SkillTypeInfo { id = 3, name = "B" });
            Automaton.Instance.WorkingAgents[agent] = macro;
            Automaton.Instance.FarmingCreatures.Add(creature);

            var before = Automaton.Instance.CurrentPostureEnum;
            Automaton.Instance.TogglePosture();
            Assert.NotEqual(before, Automaton.Instance.CurrentPostureEnum);
            // 姿态切换不清生产状态：宏/种地保留，切回自动恢复
            Assert.Same(macro, Automaton.Instance.WorkingAgents[agent]);
            Assert.Contains(creature, Automaton.Instance.FarmingCreatures);

            Automaton.Instance.TogglePosture();
            Assert.Equal(before, Automaton.Instance.CurrentPostureEnum);
            ResetProductionState();
        }

        [Fact]
        public void SwitchPostureAnnouncesThroughSystemLog()
        {
            InjectManagers();
            var captured = new List<object[]>();
            Notice.instance.Observe("AddSystemLog", param => captured.Add(param));

            var before = Automaton.Instance.CurrentPostureEnum;
            Automaton.Instance.SwitchPosture(before);
            Assert.Empty(captured); // 同姿态切换幂等，无播报

            // 切英语跑一遍三语解析，再恢复简中（ShyLook 系用例断言中文文案，语言不能带出去）
            var globalGameManager = (GlobalGameManager)typeof(GlobalGameManager)
                .GetField("_instance", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .GetValue(null);
            try
            {
                GameBridge.Set(globalGameManager, "_language", SystemLanguage.English);
                var englishSnapshot = captured.Count;
                Automaton.Instance.SwitchPosture(before == PostureEnum.PRODUCTION ? PostureEnum.SUPPRESS : PostureEnum.PRODUCTION);
                Assert.Equal("The capable move in, the rest fall back to safety. Do not worry, today's loss report will not carry any new names.", captured[captured.Count - 1][0]);
                GameBridge.Set(globalGameManager, "_language", SystemLanguage.ChineseSimplified);
                Automaton.Instance.SwitchPosture(before);
                Assert.True(captured.Count >= englishSnapshot + 2);
            }
            finally
            {
                GameBridge.Set(globalGameManager, "_language", SystemLanguage.ChineseSimplified);
            }
        }

        [Fact]
        public void ProductionPostureSeedsMacrosFarmsAndThreats()
        {
            InjectManagers();
            DrainQueue();
            ForcePosture(PostureEnum.PRODUCTION);
            ResetProductionState();

            var agent = GameBridge.CanWorkAgent();
            var farmCreature = BridgedCreature(CreatureState.WAIT, overloaded: false);
            var macro = new RepeatWorkCommand(agent);
            macro.Update(BridgedCreature(CreatureState.WAIT, overloaded: false), new SkillTypeInfo { id = 3, name = "B" });
            Automaton.Instance.WorkingAgents[agent] = macro;
            Automaton.Instance.FarmingCreatures.Add(farmCreature);

            var automaton = Automaton.Instance;
            automaton.CurrentPosture.OnTick();
            // 无威胁：seed = 宏 1 + 种地 1
            Assert.Equal(2, QueueCount());
            ResetProductionState();
        }

        [Fact]
        public void SuppressPostureDoesNotSeedProductionCommands()
        {
            InjectManagers();
            DrainQueue();
            ForcePosture(PostureEnum.SUPPRESS);
            ResetProductionState();

            var agent = GameBridge.CanWorkAgent();
            var macro = new RepeatWorkCommand(agent);
            macro.Update(BridgedCreature(CreatureState.WAIT, overloaded: false), new SkillTypeInfo { id = 3, name = "B" });
            Automaton.Instance.WorkingAgents[agent] = macro;
            Automaton.Instance.FarmingCreatures.Add(BridgedCreature(CreatureState.WAIT, overloaded: false));

            Automaton.Instance.CurrentPosture.OnTick();
            // 镇压姿态不 seed 宏/种地；空管理器下也无威胁可入队
            Assert.Equal(0, QueueCount());
            ResetProductionState();
        }

        [Fact]
        public void DrainedQueueLeavesNoResidueAcrossTicks()
        {
            InjectManagers();
            DrainQueue();
            ForcePosture(PostureEnum.PRODUCTION);
            ResetProductionState();

            // 永不解决的威胁：过载未出逃（→ManageCreatureCommand）+ 出逃（→SuppressCommand），
            // 空员工名单下 Execute 均失败——旧回炉模型会逐 tick 累积等值副本（O(T²)），排空模型必须归零
            var urgent = BridgedCreature(CreatureState.WAIT, overloaded: true);
            var escapee = BridgedCreature(CreatureState.ESCAPE, overloaded: false);
            var creatureManager = (CreatureManager)typeof(CreatureManager)
                .GetField("_instance", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .GetValue(null);
            GameBridge.Set(creatureManager, "creatureList", new List<CreatureModel> { urgent, escapee });

            var automaton = Automaton.Instance;
            // 已完成的宏（agent 已不在宏表）：入队后在执行段走 IsCompleted→continue 分支丢弃
            var finished = new RepeatWorkCommand(GameBridge.CanWorkAgent());
            finished.Update(BridgedCreature(CreatureState.WAIT, overloaded: false), new SkillTypeInfo { id = 3, name = "B" });
            automaton.Enqueue(finished);
            for (var tick = 0; tick < 5; tick++)
            {
                automaton.Main();
                Assert.Equal(0, QueueCount());
            }
            ResetProductionState();
        }

        [Fact]
        public void SuppressedPostureEnqueuesSuppressCommandForFightables()
        {
            InjectManagers();
            DrainQueue();
            ForcePosture(PostureEnum.SUPPRESS);

            var escapee = BridgedCreature(CreatureState.ESCAPE, overloaded: false);
            var creatureManager = (CreatureManager)typeof(CreatureManager)
                .GetField("_instance", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .GetValue(null);
            GameBridge.Set(creatureManager, "creatureList", new List<CreatureModel> { escapee });

            Automaton.Instance.CurrentPosture.OnTick();
            // 镇压姿态对战斗目标（低风险出逃）仍然入队镇压
            Assert.Equal(1, QueueCount());
        }

        [Fact]
        public void KeyBindingBInvokesTogglePosture()
        {
            InjectManagers();
            var bindings = typeof(Harmony_Patch)
                .GetField("_keyBindings", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .GetValue(null) as System.Collections.IEnumerable; // KeyBinding 是 struct，只能走非泛型枚举
            Assert.NotNull(bindings);
            object onDown = null;
            foreach (var binding in bindings)
            {
                var flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
                if ((KeyCode)binding.GetType().GetField("Key", flags).GetValue(binding) == KeyCode.B)
                {
                    onDown = binding.GetType().GetField("OnDown", flags).GetValue(binding);
                }
            }
            Assert.NotNull(onDown);

            var before = Automaton.Instance.CurrentPostureEnum;
            ((System.Delegate)onDown).DynamicInvoke(new object[] { null }); // B 键真实入口（Invoke 包 try/catch，离线安全）
            Assert.NotEqual(before, Automaton.Instance.CurrentPostureEnum);
        }
    }
}
