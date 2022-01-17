using System;
using System.Collections.Generic;
using System.Linq;

using AutoInority.Extentions;

namespace AutoInority
{
    internal partial class Automaton
    {
        private static Automaton _instance;

        private readonly Dictionary<long, List<string>> _caretakers = new Dictionary<long, List<string>>();

        private readonly Dictionary<string, Macro> _macros = new Dictionary<string, Macro>();

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

        private bool All { get; set; } = false;

        private bool Running { get; set; } = true;

        public static void IncreaseOverloadLevel()
        {
            Log.Info("increase overload level");
        }

        public static void Reset()
        {
            _instance = null;
        }

        public void AgentTakeDamage(AgentModel agent, DamageInfo dmg)
        {
            if (agent.hp < 0.25 * agent.maxHp || agent.mental < 0.25 * agent.maxMental)
            {
                agent.ReturnToSefira();
            }
        }

        /// <summary>
        /// Clear all macros.
        /// </summary>
        public void Clear()
        {
            foreach (var macro in _macros.Values)
            {
                Remove(macro.Agent);
            }
        }

        /// <summary>
        /// Try finding an agent to ingratiate himself with the creature.
        /// </summary>
        /// <param name="creature"></param>
        public void Creature(CreatureModel creature)
        {
            if (!Running)
            {
                return;
            }

            if (_caretakers.TryGetValue(creature.metaInfo.id, out var names))
            {
                foreach (var name in names)
                {
                    if (_macros.TryGetValue(name, out var macro) && macro.IsAvailable() && macro.IsConfident())
                    {
                        macro.Apply();
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Complete the macro if the agent got the target EGO gist.
        /// </summary>
        /// <param name="agent"></param>
        public void FinishWork(UseSkill item)
        {
            var agent = item.agent;
            var skill = item.skillTypeInfo;

            if (_macros.TryGetValue(agent.name, out var macro))
            {
                if (macro.ForGift && agent.HasEGOGift(macro.Creature, out var gift))
                {
                    Notice.instance.Send("AddSystemLog", $"{agent.Tag()}获得了{gift.equipTypeInfo.Tag()}");
                    Remove(agent);
                }
                else if (macro.ForExp && agent.HasReachedExpLimit(skill.rwbpType, out var name))
                {
                    Notice.instance.Send("AddSystemLog", $"{agent.Tag()}的{name}能力已经达到上限");
                    Remove(agent);
                }
            }
        }

        public void OrdealManager(OrdealManager manager)
        {
            if (!Running)
            {
                return;
            }

            foreach (var creature in manager.GetOrdealCreatureList())
            {
                OrdealCreature(creature);
            }
        }

        public void Register(AgentModel agent, CreatureModel creature, SkillTypeInfo skill, bool forGift = false, bool forExp = false)
        {
            try
            {
                _macros[agent.name] = new Macro()
                {
                    Agent = agent,
                    Creature = creature,
                    Skill = skill,
                    ForExp = forExp,
                    ForGift = forGift,
                };
                if (_caretakers.TryGetValue(creature.metaInfo.id, out var agents))
                {
                    agents.Add(agent.name);
                }
                else
                {
                    _caretakers.Add(creature.metaInfo.id, new List<string>() { agent.name });
                }
                Notice.instance.Send("AddSystemLog", $"{agent.Tag()}将会自动对{creature.Tag()}进行{skill.Tag()}");
                agent.AddUnitBuf(new AutomatonBuf(creature));
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                Log.Error(e.StackTrace);
            }
            finally
            {
                Log.Info("work registered");
            }
        }

        /// <summary>
        /// Enter farm mode
        /// </summary>
        /// <param name="creature"></param>
        public void Register(CreatureModel creature)
        {
        }

        /// <summary>
        /// Remove the macro
        /// </summary>
        /// <param name="agent"></param>
        public void Remove(AgentModel agent)
        {
            if (_macros.TryGetValue(agent.name, out var macro))
            {
                _macros.Remove(agent.name);
                if (_caretakers.TryGetValue(macro.Creature.metaInfo.id, out var names))
                {
                    _caretakers[macro.Creature.metaInfo.id] = new List<string>(names.Where(name => name != agent.name));
                }
            }
            foreach (var buf in agent.GetUnitBufList().Where(buf => buf is AutomatonBuf).ToList())
            {
                agent.RemoveUnitBuf(buf);
            }
        }

        /// <summary>
        /// Leave farm mode.
        /// </summary>
        /// <param name="creature"></param>
        public void Remove(CreatureModel creature)
        {

        }

        public void Sefira(Sefira sefira)
        {
            if (!Running)
            {
                return;
            }
            ManageCreatures(sefira);
            ManageCreatureKits(sefira);
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
        /// Toggle the running status of the Automaton.
        /// </summary>
        public void ToggleAll()
        {
            All = !All;
            var message = AutomationMessage();
            Angela.Say(message);
        }

        private string AutomationMessage() => Running ? (All ? Angela.Automaton.All : Angela.Automaton.On) : Angela.Automaton.Off;

        private void HandleCandidates(IEnumerable<Candidate> candidates, HashSet<CreatureModel> creatures)
        {
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
                        candidate.Apply();
                        creatures.Remove(candidate.Creature);
                    }
                }
            }
        }

        private void ManageCreatureKits(Sefira sefira)
        {
            // TODO
        }

        private void ManageCreatures(Sefira sefira)
        {
            var agents = sefira.AvailableAgents();
            var creatures = new HashSet<CreatureModel>(sefira.UrgentCreatures());

            var candidates = Candidate.Suggest(agents, creatures);
            Log.Info(sefira.name, $"Candidates: {candidates.Count()}");
            HandleCandidates(candidates, creatures);

            if (creatures.Count > 0) // call neighbor depts agents
            {
                agents = sefira.NeibourAgents();
                Log.Info(sefira.name, $"Neighbor agents: {agents.Count()}");
                candidates = Candidate.Suggest(agents, creatures);
                Log.Info(sefira.name, $"Candidates: {candidates.Count()}");
                HandleCandidates(candidates, creatures);
            }

            if (Instance.All && sefira.sefiraEnum != SefiraEnum.TIPERERTH1 && sefira.sefiraEnum != SefiraEnum.TIPERERTH2) // assign other works
            {
                creatures = new HashSet<CreatureModel>(sefira.Creatures());
                Log.Info(sefira.name, $"Remain creatures: {creatures.Count()}");
                candidates = Candidate.Suggest(agents.Where(x => x.IsAvailable()), creatures);
                Log.Info(sefira.name, $"Candidates: {candidates.Count()}");
                HandleCandidates(candidates, creatures);
            }
        }

        private void OrdealCreature(OrdealCreatureModel creature)
        {
            if (creature.state == CreatureState.SUPPRESSED || creature.state == CreatureState.SUPPRESSED_RETURN)
            {
                return;
            }
            var riskLevel = creature.GetRiskLevel();
            var sefira = creature.sefira;

            // Log.Warning($"{nameof(OrdealCreature)}: {creature.metaInfo.name}, risk level: {riskLevel}");
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
                    foreach (var agent in sefira.agentList.Where(x => x.IsAvailable() && x.IsCapableOfPressing(creature)))
                    {
                        // Log.Info($"{agent.name} (lv {agent.level}) starts suppressing {creature.metaInfo.name} with {agent.Equipment.armor.metaInfo.grade} armor");
                        agent.Suppress(creature);
                    }
                    return;
            }
        }

        private sealed class Macro
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
                return Creature.GetCreatureExtension().CanWorkWith(Agent, Skill, out _) && Agent.IsAvailable() && Creature.IsAvailable();
            }

            public bool IsConfident() => Creature.GetCreatureExtension().CheckConfidence(Agent, Skill);
        }
    }
}