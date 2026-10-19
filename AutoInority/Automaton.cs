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