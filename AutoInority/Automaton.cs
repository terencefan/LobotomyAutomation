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

        public static bool InEmergency
        {
            get
            {
                // 考验在场或存在过载收容物都视为紧急，暂停宏与种地（原实现误用 &&，仅两者同时发生才暂停）。
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

        public float CounterDecreaseConfidence { get; set; } = 0.95f;

        public float CreatureEscapeConfidence { get; set; } = 0.99f;

        public float DeadConfidence { get; set; } = 0.99f;

        public Dictionary<AgentModel, RepeatWorkCommand> WorkingAgents { get; } = new Dictionary<AgentModel, RepeatWorkCommand>();

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
        /// Clear all macros.
        /// </summary>
        public void Clear()
        {
            WorkingAgents.ToList().ForEach(x => x.Key.ClearAutomationBuf());
            WorkingAgents.Clear();
            FarmingCreatures.ToList().ForEach(x => TurnOffFarm(x.Unit.room));
            FarmingCreatures.Clear();
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
                    model.unconAction.OnClick();
                }
            }
        }

        public void Main()
        {
            if (!Running)
            {
                return;
            }

            // 每 tick 从源头重建命令队列：命令是瞬态的，未完成/未适用的下个 tick
            // 由源头重新生成。旧实现的 _repeatQueue/_commands 组合存在增长缺陷
            // （去重集合只删不增，失败命令每 tick 复制一份，长期紧急时 O(T²) 累积）
            var commands = new List<ICommand>();
            commands.AddRange(WorkingAgents.Values.Cast<ICommand>());

            foreach (var creature in FarmingCreatures)
            {
                commands.Add(new FarmCommand(creature));
            }

            foreach (var kit in CreatureManager.instance.GetCreatureList().Where(x => x.IsKit()))
            {
                if (kit.isOverloaded)
                {
                    commands.Add(new ManageKitCommand(kit));
                }
                kit.GetKitExtension().OnFixedUpdate();
            }

            foreach (var creature in CreatureManager.instance.GetCreatureList().FilterUrgent())
            {
                commands.Add(new ManageCreatureCommand(creature));
            }

            foreach (var creature in CreatureManager.instance.GetCreatureList().Where(x => x.state == CreatureState.ESCAPE))
            {
                if (creature.GetExtension().AutoSuppress)
                {
                    commands.Add(new SuppressCommand(creature));
                }
            }

            foreach (var creature in OrdealManager.instance.GetOrdealCreatureList())
            {
                commands.Add(new SuppressCommand(creature));
            }

            var queue = new PriorityQueue<ICommand>();
            foreach (var command in commands)
            {
                queue.Enqueue(command);
            }

            while (queue.Count() > 0)
            {
                var command = queue.Dequeue();
                if (!command.IsCompleted && command.IsApplicable)
                {
                    command.Execute();
                }
            }
        }

        public void Register(AgentModel agent, CreatureModel creature, SkillTypeInfo skill, bool forGift = false, bool forExp = false)
        {
            if (WorkingAgents.TryGetValue(agent, out var command))
            {
                agent.ClearAutomationBuf();
            }
            else
            {
                command = new RepeatWorkCommand(agent);
                WorkingAgents[agent] = command;
            }
            command.Update(creature, skill, forGift, forExp);
            agent.AddUnitBuf(new AutomatonBuf(creature));
            Notice.instance.Send("AddSystemLog", $"{agent.Tag()}将会自动对{creature.Tag()}进行{skill.Tag()}");
        }

        /// <summary>
        /// Remove the macro
        /// </summary>
        /// <param name="agent"></param>
        public void Remove(AgentModel agent)
        {
            WorkingAgents.Remove(agent);
            agent.ClearAutomationBuf();
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

        private void HandleKitEvents(CreatureModel kit)
        {
            var ext = kit.GetKitExtension();
            ext.OnFixedUpdate();
        }
    }
}