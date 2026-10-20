using System.Collections.Generic;
using System.Reflection;

using AutoInority.Command;
using AutoInority.Extentions;
using AutoInority.Posture;

using UnityEngine;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 姿态调度骨架：B 键三态循环、三姿态 OnTick 的 seed 语义、应急反射公共基座、
    /// Main 排空不回炉。
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
        internal static void InjectManagers()
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
        internal static CreatureModel BridgedCreature(CreatureState state, bool overloaded)
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

        /// <summary>未满级员工：MaxStat 链离线要求 MissionManager bare + 空任务列表（跨类共享静态，
        /// TrainableTypesTest cctor 已注，此处置空防用例顺序）。</summary>
        private static AgentModel TrainableAgent()
        {
            InjectMissionManager();
            var agent = GameBridge.CanWorkAgent();
            // primaryStat + _statBufList 双注（YouMustBeHappyExtTest 先例）：
            // TotalStats→maxHp→GetMaxHpBuf 遍历 _statBufList，漏注即 NRE
            GameBridge.Set(agent, "primaryStat", new WorkerPrimaryStat { hp = 10, mental = 10, work = 10, battle = 10 });
            GameBridge.Set(agent, "primaryStatExp", new WorkerPrimaryStatExp());
            GameBridge.Set(agent, "_statBufList", new List<UnitStatBuf>());
            return agent;
        }

        private static AgentModel MaxedAgent()
        {
            InjectMissionManager();
            var agent = GameBridge.CanWorkAgent();
            GameBridge.Set(agent, "primaryStat", new WorkerPrimaryStat { hp = 130, mental = 130, work = 130, battle = 130 });
            GameBridge.Set(agent, "primaryStatExp", new WorkerPrimaryStatExp());
            GameBridge.Set(agent, "_statBufList", new List<UnitStatBuf>());
            agent.hp = agent.maxHp;
            agent.mental = agent.maxMental;
            return agent;
        }

        private static void InjectMissionManager()
        {
            var existing = typeof(MissionManager)
                .GetField("_instance", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .GetValue(null);
            if (existing != null)
            {
                return;
            }
            var missionManager = GameBridge.Bare<MissionManager>();
            typeof(MissionManager)
                .GetField("_instance", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .SetValue(null, missionManager);
            GameBridge.Set(missionManager, "clearedMissions", new List<Mission>());
            GameBridge.Set(missionManager, "closedMissions", new List<Mission>());
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
            Automaton.Instance.ExploitingCreatures.Clear();
        }

        private static bool SkipPrefix() => false;

        [Fact]
        public void TogglePostureFlipsEnumAndKeepsState()
        {
            InjectManagers();
            var creature = BridgedCreature(CreatureState.WAIT, overloaded: false);
            Automaton.Instance.ExploitingCreatures.Add(creature);

            var before = Automaton.Instance.CurrentPostureEnum;
            Automaton.Instance.TogglePosture();
            Assert.NotEqual(before, Automaton.Instance.CurrentPostureEnum);
            // 姿态切换不清生产状态：开发目标保留，切回自动恢复
            Assert.Contains(creature, Automaton.Instance.ExploitingCreatures);

            ForcePosture(before);
            ResetProductionState();
        }

        [Fact]
        public void TogglePostureCyclesThroughThreeStates()
        {
            InjectManagers();
            var start = Automaton.Instance.CurrentPostureEnum;
            try
            {
                ForcePosture(PostureEnum.PRODUCTION);
                Automaton.Instance.TogglePosture();
                Assert.Equal(PostureEnum.TRAINING, Automaton.Instance.CurrentPostureEnum);
                Automaton.Instance.TogglePosture();
                Assert.Equal(PostureEnum.SUPPRESS, Automaton.Instance.CurrentPostureEnum);
                Automaton.Instance.TogglePosture();
                Assert.Equal(PostureEnum.PRODUCTION, Automaton.Instance.CurrentPostureEnum); // B×3 回原点
            }
            finally
            {
                ForcePosture(start);
            }
        }

        [Fact]
        public void SwitchPostureAnnouncesThroughSystemLog()
        {
            InjectManagers();
            ForcePosture(PostureEnum.PRODUCTION); // 先就位：切换播报不能落进捕获窗
            var captured = new List<object[]>();
            Notice.instance.Observe("AddSystemLog", param => captured.Add(param));

            Automaton.Instance.SwitchPosture(PostureEnum.PRODUCTION);
            Assert.Empty(captured); // 同姿态切换幂等，无播报

            // 切英语跑一遍三语解析，再恢复简中（ShyLook 系用例断言中文文案，语言不能带出去）
            var globalGameManager = (GlobalGameManager)typeof(GlobalGameManager)
                .GetField("_instance", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .GetValue(null);
            try
            {
                GameBridge.Set(globalGameManager, "_language", SystemLanguage.English);
                Automaton.Instance.SwitchPosture(PostureEnum.SUPPRESS);
                Assert.Equal("The capable move in, the rest fall back to safety. Do not worry, today's loss report will not carry any new names.", captured[captured.Count - 1][0]);
                GameBridge.Set(globalGameManager, "_language", SystemLanguage.ChineseSimplified);
                // 训练姿态文案键独立于生产/镇压，中文断言钉住
                Automaton.Instance.SwitchPosture(PostureEnum.TRAINING);
                Assert.Equal("设施里成长最慢的那几位，今天由我亲自安排课程。请相信，合适的位置比盲目的努力更能改变一个人。", captured[captured.Count - 1][0]);
            }
            finally
            {
                GameBridge.Set(globalGameManager, "_language", SystemLanguage.ChineseSimplified);
            }
        }

        [Fact]
        public void ProductionPostureSeedsExploitsAndThreats()
        {
            InjectManagers();
            DrainQueue();
            ForcePosture(PostureEnum.PRODUCTION);
            ResetProductionState();

            var exploitCreature = BridgedCreature(CreatureState.WAIT, overloaded: false);
            Automaton.Instance.ExploitingCreatures.Add(exploitCreature);

            var automaton = Automaton.Instance;
            automaton.CurrentPosture.OnTick();
            // 无威胁：seed = 开发 1
            Assert.Equal(1, QueueCount());
            ResetProductionState();
        }

        [Fact]
        public void SuppressPostureDoesNotSeedProductionCommands()
        {
            InjectManagers();
            DrainQueue();
            ForcePosture(PostureEnum.SUPPRESS);
            ResetProductionState();

            Automaton.Instance.ExploitingCreatures.Add(BridgedCreature(CreatureState.WAIT, overloaded: false));

            Automaton.Instance.CurrentPosture.OnTick();
            // 镇压姿态不 seed 开发；空管理器下也无威胁可入队
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
            for (var tick = 0; tick < 5; tick++)
            {
                automaton.Main();
                Assert.Equal(0, QueueCount());
            }
            ResetProductionState();
        }

        [Fact]
        public void BaseEnqueuesSuppressCommandForFightables()
        {
            InjectManagers();
            DrainQueue();

            var escapee = BridgedCreature(CreatureState.ESCAPE, overloaded: false);
            var creatureManager = (CreatureManager)typeof(CreatureManager)
                .GetField("_instance", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .GetValue(null);
            GameBridge.Set(creatureManager, "creatureList", new List<CreatureModel> { escapee });

            // 威胁扫描是三姿态公共基座：与当前姿态无关，出逃低风险目标入队镇压
            Automaton.Instance.EnqueueThreats();
            Assert.Equal(1, QueueCount());
        }

        [Fact]
        public void ThreatBaseWorksWithAnyPosture()
        {
            InjectManagers();
            DrainQueue();

            var urgent = BridgedCreature(CreatureState.WAIT, overloaded: true);
            var escapee = BridgedCreature(CreatureState.ESCAPE, overloaded: false);
            var creatureManager = (CreatureManager)typeof(CreatureManager)
                .GetField("_instance", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .GetValue(null);
            GameBridge.Set(creatureManager, "creatureList", new List<CreatureModel> { urgent, escapee });

            // 基座断言（核心）：三姿态任一下应急反射都产出威胁命令
            foreach (var posture in new[] { PostureEnum.PRODUCTION, PostureEnum.TRAINING, PostureEnum.SUPPRESS })
            {
                DrainQueue();
                ForcePosture(posture);
                Automaton.Instance.EnqueueThreats();
                Assert.Equal(2, QueueCount()); // urgent→ManageCreatureCommand + escapee→SuppressCommand
            }
        }

        [Fact]
        public void TrainingPostureSeedsTrainCommandsForTrainableAgents()
        {
            InjectManagers();
            DrainQueue();
            ForcePosture(PostureEnum.TRAINING);

            var trainable = TrainableAgent();
            var agentManager = (AgentManager)typeof(AgentManager)
                .GetField("_instance", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .GetValue(null);
            GameBridge.Set(agentManager, "agentList", new List<AgentModel> { trainable });

            // IsAvailable 门：hp/mental >= max*0.9——bare 员工当前数值默认 0，须设满
            trainable.hp = trainable.maxHp;
            trainable.mental = trainable.maxMental;

            Automaton.Instance.CurrentPosture.OnTick();
            Assert.Equal(1, QueueCount());
        }

        [Fact]
        public void TrainingPostureSkipsMaxedAgents()
        {
            InjectManagers();
            DrainQueue();
            ForcePosture(PostureEnum.TRAINING);

            var maxed = MaxedAgent();
            var agentManager = (AgentManager)typeof(AgentManager)
                .GetField("_instance", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .GetValue(null);
            GameBridge.Set(agentManager, "agentList", new List<AgentModel> { maxed });

            Automaton.Instance.CurrentPosture.OnTick();
            Assert.Equal(0, QueueCount()); // 全满级者无经验缺口，不入队
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
