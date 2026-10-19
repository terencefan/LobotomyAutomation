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

        private readonly HashSet<ICommand> _commands = new HashSet<ICommand>();

        private readonly Queue<ICommand> _repeatQueue = new Queue<ICommand>();

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

        // 心之引信主动回送：带心员工被手工派往非女巫目标时，5-10s 引信在途即可引爆
        // （门口兜底要到门才拦），立即撤单回 Sefira 待机点
        private void RecallHeartAgents()
        {
            var witch = CreatureManager.instance.GetCreatureList().FirstOrDefault(x => x.script is LittleWitch);
            LaetitiaExt.RecallHeartAgents(AgentManager.instance.GetAgentList(), witch);
        }

        public void Main()
        {
            if (!Running)
            {
                return;
            }

            while (_repeatQueue.Count > 0)
            {
                Enqueue(_repeatQueue.Dequeue());
            }

            foreach (var kit in CreatureManager.instance.GetCreatureList().Where(x => x.IsKit()))
            {
                if (kit.isOverloaded)
                {
                    Enqueue(new ManageKitCommand(kit));
                }
                kit.GetKitExtension().OnFixedUpdate();
            }

            foreach (var creature in CreatureManager.instance.GetCreatureList().FilterUrgent())
            {
                Enqueue(new ManageCreatureCommand(creature));
            }

            RecallHeartAgents();

            foreach (var creature in CreatureManager.instance.GetCreatureList().Where(x => x.state == CreatureState.ESCAPE))
            {
                // HE 及以上本体不自动镇压是既有姿态策略；子实体不受此限——
                // 女巫之子 actively 伤人且镇压对本体零影响，清场不越权
                if (creature.GetExtension().AutoSuppress)
                {
                    Enqueue(new SuppressCommand(creature));
                }
                foreach (var child in creature.GetExtension().SuppressionTargets)
                {
                    Enqueue(new SuppressCommand(child));
                }
            }

            foreach (var creature in OrdealManager.instance.GetOrdealCreatureList())
            {
                Enqueue(new SuppressCommand(creature));
            }

            while (_commandQueue.Count() > 0)
            {
                var command = _commandQueue.Dequeue();
                if (command.IsCompleted)
                {
                    _commands.Remove(command);
                }
                else if (command.IsApplicable && command.Execute())
                {
                    _repeatQueue.Enqueue(command); // TODO wait 3+ cycles
                }
                else
                {
                    _repeatQueue.Enqueue(command); // check on next cycle
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

        private void Enqueue(ICommand command)
        {
            if (_commands.Contains(command))
            {
                return;
            }
            _commandQueue.Enqueue(command);
        }

        private void HandleKitEvents(CreatureModel kit)
        {
            var ext = kit.GetKitExtension();
            ext.OnFixedUpdate();
        }
    }
}