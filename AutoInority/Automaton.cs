using System.Collections.Generic;
using System.Linq;

using AutoInority.Extentions;

namespace AutoInority
{
    internal partial class Automaton
    {
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

        internal HashSet<CreatureModel> FarmingCreatures { get; } = new HashSet<CreatureModel>();

        internal Dictionary<CreatureModel, List<Macro>> MacroCreatures { get; } = new Dictionary<CreatureModel, List<Macro>>();

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
                        var message = string.Format(Angela.Agent.GotEGOGift, agent.Tag(), gift.equipTypeInfo.Tag());
                        Angela.Log(message);
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

        public bool ManageCreatures(CreatureManager manager)
        {
            if (!Running)
            {
                return true;
            }

            // handle Qliphoth meltdown events (and some other urgent events)
            for (int riskLevel = 5; riskLevel > 0; riskLevel--)
            {
                var creatures = manager.GetCreatureList().FilterUrgent(riskLevel).ToHashSet();

                // find from current dtps
                var candidates = creatures.FindCandidates();
                if (HandleCandidates(candidates, creatures))
                {
                    return true;
                }

                // find from neighbor depts
                candidates = creatures.FindCandidates(true);
                if (HandleCandidates(candidates, creatures))
                {
                    return true;
                }
            }

            // suppress escaping creatures (only a few of them)
            foreach (var creature in manager.GetCreatureList().Where(x => x.state == CreatureState.ESCAPE))
            {
                Log.Info($"{creature.metaInfo.name} is escaping");
                if (creature.GetCreatureExtension().AutoSuppress)
                {
                    creature.sefira.agentList.FilterCanSuppress(creature).ToList().ForEach(x => x.Suppress(creature));
                }
            }

            // parse macro / farm when handling ordeals.
            if (OrdealManager.instance.GetOrdealCreatureList().Where(x => x.state != CreatureState.SUPPRESSED).Any())
            {
                return true;
            }

            // assign only one work per cycle.
            return TryRunMacro() || TryFarm() || TryFarm(true);
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
                MacroCreatures[entry.Key] = entry.Value.Where(x => x.Agent != agent).ToList();
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
            if (FarmingCreatures.Remove(creature))
            {
                var message = string.Format(Angela.Automaton.FarmOff, creature.Tag());
                Angela.Log(message);
            }
            else
            {
                FarmingCreatures.Add(creature);
                var message = string.Format(Angela.Automaton.FarmOn, creature.Tag());
                Angela.Log(message);
            }
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
                        return true;
                    }
                }
            }
            return false;
        }

        private bool TryFarm(bool neighbor = false)
        {
            foreach (var creature in FarmingCreatures)
            {
                var agents = neighbor ? creature.sefira.NeighborAgents() : creature.sefira.agentList;
                agents = agents.FilterEGOGift(creature);

                var candidates = Candidate.Suggest(agents, new[] { creature });

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
                    var agents = sefira.agentList.FilterCanSuppress(creature).ToList();
                    if (agents.Count > 0)
                    {
                        agents.ForEach(x => x.Suppress(creature));
                    }
                    else
                    {
                        agents = sefira.NeighborAgents().FilterCanSuppress(creature).ToList();
                        agents.ForEach(x => x.Suppress(creature));
                    }
                    return;
            }
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
                return Creature.GetCreatureExtension().CanWorkWith(Agent, Skill, out _) && Agent.IsAvailable() && Creature.IsAvailable();
            }

            public bool IsConfident() => Creature.GetCreatureExtension().CheckConfidence(Agent, Skill);
        }
    }
}