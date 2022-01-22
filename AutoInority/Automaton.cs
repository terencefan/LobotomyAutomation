using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using AutoInority.Creature;
using AutoInority.Extentions;

using UnityEngine;

namespace AutoInority
{
    internal partial class Automaton
    {
        public const float FarmingAnimSpeed = 0.2f;

        private static Automaton _instance;

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

        public bool InEmergency
        {
            get
            {
                if (OrdealManager.instance.GetOrdealCreatureList().Where(x => x.state != CreatureState.SUPPRESSED).Any())
                {
                    return true;
                }
                return false;
            }
        }

        internal HashSet<CreatureModel> FarmingCreatures { get; } = new HashSet<CreatureModel>();

        internal Dictionary<CreatureModel, List<Macro>> MacroCreatures { get; } = new Dictionary<CreatureModel, List<Macro>>();

        private bool Running { get; set; } = true;

        public static void Reset()
        {
            _instance = null;
        }

        public void AgentAttachEGOgift(AgentModel agent, EGOgiftModel gift)
        {
            if (agent.EGOSlotLocked(gift.metaInfo, out var slotName))
            {
                return;
            }
            else if (gift.metaInfo.id != SnowQueenExt.DummyGiftId)
            {
                Angela.Log(string.Format(Angela.Agent.GotEGOGift, agent.Tag(), gift.metaInfo.Tag()));
            }
        }

        public void AgentTakeDamage(AgentModel agent, DamageInfo dmg)
        {
            if (agent.GetState() == AgentAIState.SUPPRESS_CREATURE && agent.hp < 0.25 * agent.maxHp || agent.mental < 0.25 * agent.maxMental)
            {
                agent.ReturnToSefira();
            }
        }

        /// <summary>
        /// Clear all macros.
        /// </summary>
        public void Clear()
        {
            MacroCreatures.Clear();
            FarmingCreatures.Clear();
            AgentManager.instance.GetAgentList().ToList().ForEach(x => x.RemoveAutomatonBuff());
        }

        /// <summary>
        /// Complete the macro if the agent got the target EGO gist.
        /// </summary>
        /// <param name="agent"></param>
        public void FinishWork(UseSkill item)
        {
            var agent = item.agent;
            var skill = item.skillTypeInfo;

            if (MacroCreatures.TryGetValue(item.targetCreature, out var macros))
            {
                foreach (var macro in macros)
                {
                    if (agent != macro.Agent)
                    {
                        continue;
                    }
                    else if (macro.ForGift && agent.HasEGOGift(macro.Creature, out var gift))
                    {
                        Remove(agent);
                    }
                    else if (macro.ForExp && agent.HasReachedExpLimit(skill.rwbpType, out var skillName))
                    {
                        var message = string.Format(Angela.Agent.ReachMaxExp, agent.Tag(), skillName);
                        Angela.Log(message);
                        Remove(agent);
                    }
                }
            }
        }

        public void ManageCreatures(CreatureManager manager)
        {
            if (!Running)
            {
                return;
            }

            // handle Qliphoth meltdown events (and some other urgent events)
            for (int riskLevel = 5; riskLevel > 0; riskLevel--)
            {
                var creatures = new HashSet<CreatureModel>(manager.GetCreatureList().FilterUrgent(riskLevel));

                // find from current dtps
                var candidates = creatures.FindCandidates();
                candidates.Sort(Candidate.Comparer);

                if (HandleCandidates(candidates, creatures))
                {
                    Log.Debug($"handle emergency risk level {riskLevel}");
                    return;
                }

                // find from neighbor depts
                candidates = creatures.FindCandidates(true);
                candidates.Sort(Candidate.Comparer);
                if (HandleCandidates(candidates, creatures))
                {
                    Log.Debug($"handle emergency risk level {riskLevel}, extend");
                    return;
                }
            }

            // auto suppress escaping creatures (only a few of them)
            foreach (var creature in manager.GetCreatureList().Where(x => x.state == CreatureState.ESCAPE))
            {
                Log.Debug($"{creature.metaInfo.name} escaped.");
                if (creature.GetExtension().AutoSuppress)
                {
                    creature.GetExtension().FindAgents().FilterCanSuppress(creature).ToList().ForEach(x => x.Suppress(creature));
                }
            }

            // auto suppress ordeal creatures.
            foreach (var creature in OrdealManager.instance.GetOrdealCreatureList())
            {
                SuppressOrdealCreature(creature);
            }

            // parse macro / farm when handling ordeals.
            if (InEmergency)
            {
                Log.Debug($"In emergency");
                return;
            }

            // assign up to 5 works per cycle.
            int i = 0;
            for (; i < 5; i++)
            {
                var result = TryRunMacro() || TryFarm();
                if (!result)
                {
                    break;
                }
            }
            Log.Debug($"{i} works assigned in this cycle.");
        }

        public void ManageOrdealCreatures(OrdealManager manager)
        {
            if (!Running)
            {
                return;
            }

            foreach (var creature in manager.GetOrdealCreatureList())
            {
                SuppressOrdealCreature(creature);
            }
        }

        public void ManageSefira(SefiraManager manager)
        {
            if (!Running)
            {
                return;
            }

            var sefiras = manager.GetOpendSefiraList().ToList();
            sefiras.Sort((x, y) => x.GetPriority().CompareTo(y.GetPriority()));

            foreach (var sefira in sefiras)
            {
                // TODO
            }
        }

        public void OnCancelWork(IsolateRoom room)
        {
            FarmingCreatures.Remove(room.TargetUnit.model);
            var animator = room.CurrentWorkRoot.GetComponent<Animator>();
            animator.speed = 0.5f; // set play speed back to normal
            animator.SetTrigger("Stop");
            room.TurnOffRoomLight();
        }

        public bool OnEnterRoom(IsolateRoom room, AgentModel worker, UseSkill skill)
        {
            var animator = room.CurrentWorkRoot.GetComponent<Animator>();
            animator.speed = 0.5f; // set play speed back to normal

            if (FarmingCreatures.Where(x => x.Unit.room == room).Any())
            {
                var IsWorkAllocated = typeof(IsolateRoom).GetProperty("IsWorkAllocated", BindingFlags.NonPublic | BindingFlags.Instance);
                var IsWorking = typeof(IsolateRoom).GetProperty("IsWorking", BindingFlags.NonPublic | BindingFlags.Instance);
                var InitProcessText = typeof(IsolateRoom).GetMethod("InitProcessText", BindingFlags.NonPublic | BindingFlags.Instance);

                IsWorkAllocated.SetValue(room, false, null);
                IsWorking.SetValue(room, true, null);
                if (skill.skillTypeInfo.id != 5)
                {
                    room.DescController.Display(LocalizeTextDataModel.instance.GetText("WorkProcess_start_0"), -1);
                    room.StartWorkDesc();
                }
                InitProcessText.Invoke(room, new object[] { skill.skillTypeInfo.rwbpType });
                room.TurnOnRoomLight();
                return false;
            }
            return true;
        }

        public bool OnExitRoom(IsolateRoom room)
        {
            if (FarmingCreatures.Where(x => x.Unit.room == room).Any())
            {
                var animator = room.CurrentWorkRoot.GetComponent<Animator>();
                animator.speed = FarmingAnimSpeed;
                room.StopWorkDesc();
                return false;
            }
            return true;
        }

        public void Register(AgentModel agent, CreatureModel creature, SkillTypeInfo skill, bool forGift = false, bool forExp = false)
        {
            if (!MacroCreatures.TryGetValue(creature, out var macros))
            {
                macros = new List<Macro>();
                MacroCreatures[creature] = macros;
            }

            var macro = new Macro()
            {
                Agent = agent,
                Creature = creature,
                Skill = skill,
                ForExp = forExp,
                ForGift = forGift,
            };
            macros.Add(macro);

            Notice.instance.Send("AddSystemLog", $"{agent.Tag()}将会自动对{creature.Tag()}进行{skill.Tag()}");
            agent.AddUnitBuf(new AutomatonBuf(creature));
        }

        /// <summary>
        /// Remove the macro
        /// </summary>
        /// <param name="agent"></param>
        public void Remove(AgentModel agent)
        {
            foreach (var entry in MacroCreatures)
            {
                var macro = entry.Value.Where(x => x.Agent == agent);
                if (macro.Any())
                {
                    MacroCreatures[entry.Key].Remove(macro.First());
                }
            }
            agent.RemoveAutomatonBuff();
        }

        /// <summary>
        /// Toggle the running status of the Automaton.
        /// </summary>
        public void Toggle()
        {
            Running = !Running;
            var message = AutomationMessage();
            Angela.Say(message);
        }

        /// <summary>
        /// Enter farm mode
        /// </summary>
        /// <param name="creature"></param>
        public void ToggleFarming(CreatureModel creature)
        {
            var room = creature.Unit.room;

            if (FarmingCreatures.Remove(creature))
            {
                room.TurnOnRoomLight();
                var message = string.Format(Angela.Automaton.FarmOff, creature.Tag());
                Angela.Log(message);
            }
            else
            {
                room.CurrentWorkRoot.SetActive(value: true);
                var animator = room.CurrentWorkRoot.GetComponent<Animator>();
                animator.speed = FarmingAnimSpeed;
                animator.SetTrigger("Run");
                room.TurnOnRoomLight();

                FarmingCreatures.Add(creature);
                var message = string.Format(Angela.Automaton.FarmOn, creature.Tag());
                Angela.Log(message);
            }
        }

        private string AutomationMessage() => Running ? Angela.Automaton.On : Angela.Automaton.Off;

        private bool HandleCandidates(IEnumerable<Candidate> candidates, HashSet<CreatureModel> creatures)
        {
            int count = 0;
            foreach (var candidate in candidates)
            {
                if (creatures.Contains(candidate.Creature))
                {
                    if (!candidate.Creature.IsAvailable())
                    {
                        creatures.Remove(candidate.Creature);
                    }
                    else if (candidate.Agent.IsAvailable())
                    {
                        count++;
                        candidate.Apply();
                        creatures.Remove(candidate.Creature);
                    }
                }
            }
            return count > 0;
        }

        private void SuppressOrdealCreature(OrdealCreatureModel creature)
        {
            if (creature.state == CreatureState.SUPPRESSED || creature.state == CreatureState.SUPPRESSED_RETURN)
            {
                return;
            }
            var sefira = creature.sefira;

            switch (creature.script.GetType().Name)
            {
                case nameof(CircusDawn):
                case nameof(MachineDawn):
                case nameof(BugDawn):
                case nameof(OutterGodDawn):
                case nameof(OutterGodNoon):
                case nameof(ScavengerNoon):
                case nameof(CircusNoon):
                case nameof(MachineNoon):
                case nameof(CircusDusk):
                case nameof(MachineDusk):
                    var agents = sefira.FindNearestAgents().FilterCanSuppress(creature).ToList();
                    if (agents.Count > 0)
                    {
                        agents.ForEach(x => x.Suppress(creature));
                    }
                    else
                    {
                        agents = sefira.FindNearestAgents(true).FilterCanSuppress(creature).ToList();
                        agents.ForEach(x => x.Suppress(creature));
                    }
                    return;
            }
        }

        private bool TryFarm()
        {
            foreach (var creature in FarmingCreatures.Where(x => x.IsAvailable()))
            {
                var agents = AgentManager.instance.GetAgentList().Where(x => creature.GetExtension().FarmFilter(x));
                var candidates = Candidate.Suggest(agents, new[] { creature });
                candidates.Sort(Candidate.FarmComparer);

                foreach (var candidate in candidates)
                {
                    if (candidate.Agent.IsAvailable() && candidate.Creature.IsAvailable())
                    {
                        candidate.Apply();
                        return true;
                    }
                }
            }
            return false;
        }

        private bool TryRunMacro()
        {
            foreach (var macros in MacroCreatures.Values)
            {
                foreach (var macro in macros)
                {
                    if (macro.IsAvailable())
                    {
                        macro.Apply();
                        Log.Debug($"Repeat work on {macro.Creature.metaInfo.name}");
                        return true;
                    }
                }
            }
            return false;
        }

        internal sealed class Macro
        {
            public AgentModel Agent;

            public CreatureModel Creature;

            public bool ForExp;

            public bool ForGift;

            public SkillTypeInfo Skill;

            public void Apply()
            {
                var sprite = CommandWindow.CommandWindow.CurrentWindow.GetWorkSprite(Skill.rwbpType);
                Agent.ManageCreature(Creature, Skill, sprite);
                Agent.counterAttackEnabled = false;
                Creature.Unit.room.OnWorkAllocated(Agent);
                Creature.script.OnWorkAllocated(Skill, Agent);
                AngelaConversation.instance.MakeMessage(AngelaMessageState.MANAGE_START, Agent, Skill, Creature);
            }

            public bool IsAvailable()
            {
                return Creature.GetExtension().CanWorkWith(Agent, Skill, out _) && Agent.IsAvailable() && Creature.IsAvailable();
            }

            public bool IsConfident() => Creature.GetExtension().CheckConfidence(Agent, Skill);
        }
    }
}