using System.Collections.Generic;
using System.Linq;

using AutoInority.Command;
using AutoInority.Creature;
using AutoInority.Extentions;

namespace AutoInority
{
    internal partial class Automaton
    {
        private static Automaton _instance;

        private readonly PriorityQueue<ICommand> _commandQueue = new PriorityQueue<ICommand>();

        public static bool InEmergency
        {
            get
            {
                // 考验在场或存在过载收容物都视为紧急，暂停种地（原实现误用 &&，仅两者同时发生才暂停）。
                if (OrdealManager.instance.GetOrdealCreatureList().Where(x => x.state != CreatureState.SUPPRESSED).Any())
                {
                    return true;
                }
                return CreatureManager.instance.GetCreatureList().Where(x => x.IsAvailable() && x.isOverloaded).Any();
            }
        }

        public static Automaton Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Automaton();
                }
                return _instance;
            }
        }

        private bool Running { get; set; } = true;

        public static void Reset()
        {
            _instance = null;
        }

        public void AgentAttachEGOgift(AgentModel agent, EGOgiftModel gift)
        {
            if (agent.IsRegionLocked(gift.metaInfo))
            {
                return;
            }
            else if (gift.metaInfo.id != SnowQueenExt.DummyGiftId)
            {
                Angela.Log(string.Format(Angela.Agent.GotEGOGift, agent.Tag(), gift.metaInfo.Tag()));
            }
        }

        public void AgentOnFixedUpdate(AgentModel agent)
        {
            if (agent.CurrentPanicAction != null)
            {
                // TODO handle panic.
            }

            var state = agent.GetState();

            switch (state)
            {
                case AgentAIState.SUPPRESS_CREATURE:
                case AgentAIState.SUPPRESS_WORKER:
                case AgentAIState.SUPPRESS_OBJECT:
                    if (agent.hp < 0.3f * agent.maxHp || agent.mental < 0.3f * agent.maxMental)
                    {
                        // TODO move to sefira
                        agent.ResetWaitingPassage();
                    }
                    return;
            }
        }

        /// <summary>
        /// Clear all exploitations.
        /// </summary>
        public void Clear()
        {
            ExploitingCreatures.ToList().ForEach(x => TurnOffExploit(x.Unit.room));
            ExploitingCreatures.Clear();
        }

        public void HandleUncontrollable()
        {
            foreach (var model in WorkerManager.instance.GetWorkerList().Where(x => x.unconAction != null))
            {
                if (model.unconAction is Uncontrollable_RedShoesAttract)
                {
                    model.unconAction.OnClick();
                }
                else if (model.unconAction is Uncontrollable_Yggdrasil)
                {
                    model.unconAction.OnClick();
                }
                else if (model.unconAction is Uncontrollable_Baku)
                {
                    model.unconAction.OnClick();
                }
                else if (model.unconAction is Uncontrollable_YoungPrince)
                {
                    model.unconAction.OnClick();
                }
                else if (model.unconAction is Uncontrollable_Sakura)
                {
                    // 连点打满唤醒阈值（5 次），防员工在单点节奏下走到工作位触发献祭
                    SakuraExt.ResolveAttract(model);
                }
            }
        }

        public void Main()
        {
            if (!Running)
            {
                return;
            }

            // 共用前奏：kit 维护，两姿态都跑
            foreach (var kit in CreatureManager.instance.GetCreatureList().Where(x => x.IsKit()))
            {
                if (kit.isOverloaded)
                {
                    Enqueue(new ManageKitCommand(kit));
                }
                kit.GetKitExtension().OnFixedUpdate();
            }

            // 应急反射：三姿态公共基座——紧急收容物/出逃/考验与姿态无关，统一扫描
            EnqueueThreats();

            // 姿态分发：各姿态自行扫描入队
            CurrentPosture.OnTick();

            // 共用执行段：排空即丢弃，无回炉；等值命令下一 tick 由姿态重扫重建
            while (_commandQueue.Count() > 0)
            {
                var command = _commandQueue.Dequeue();
                if (command.IsCompleted)
                {
                    continue;
                }
                if (command.IsApplicable)
                {
                    command.Execute();
                }
            }
        }

        /// <summary>
        /// 战斗目标：出逃（AutoSuppress 放行，仅低风险）+ 全部考验。
        /// </summary>
        internal static IEnumerable<CreatureModel> GetFightables()
        {
            foreach (var creature in CreatureManager.instance.GetCreatureList().Where(x => x.state == CreatureState.ESCAPE))
            {
                if (creature.GetExtension().AutoSuppress)
                {
                    yield return creature;
                }
            }
            foreach (var creature in OrdealManager.instance.GetOrdealCreatureList())
            {
                yield return creature;
            }
        }

        /// <summary>
        /// 应急反射：紧急收容物与战斗目标的扫描入队。三姿态公共基座——
        /// 与姿态无关的威胁由 Main 统一扫描，任何姿态下紧急事都不晾着。
        /// </summary>
        internal void EnqueueThreats()
        {
            foreach (var creature in CreatureManager.instance.GetCreatureList().FilterUrgent())
            {
                Enqueue(new ManageCreatureCommand(creature));
            }
            foreach (var creature in GetFightables())
            {
                Enqueue(new SuppressCommand(creature));
            }
        }

        /// <summary>
        /// Toggle the running status of the Automaton.
        /// </summary>
        public void ToggleAutomation()
        {
            Running = !Running;
            var message = AutomationMessage();
            Angela.Announce(message);
        }

        private string AutomationMessage() => Running ? Angela.Automaton.On : Angela.Automaton.Off;

        internal void Enqueue(ICommand command)
        {
            _commandQueue.Enqueue(command);
        }
    }
}