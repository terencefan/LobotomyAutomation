using System.Collections.Generic;
using System.Linq;

using AutoInority.Creature;
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

        public bool InEmergency
        {
            get
            {
                var b1 = OrdealManager.instance.GetOrdealCreatureList().Where(x => x.state != CreatureState.SUPPRESSED).Any();
                Log.Debug("Suppressing ordeal creatures.");
                var b2 = CreatureManager.instance.GetCreatureList().Where(x => x.IsAvailable() && x.isOverloaded).Any();
                Log.Debug("Handling qliphoth meltdowns.");
                return b1 && b2;
            }
        }

        internal Dictionary<CreatureModel, List<Macro>> MacroCreatures { get; } = new Dictionary<CreatureModel, List<Macro>>();

        private bool Running { get; set; } = true;

        public static void Reset()
        {
            _instance = null;
        }

        public void AgentAttachEGOgift(AgentModel agent, EGOgiftModel gift)
        {
            if (agent.IsRegionLocked(gift.metaInfo, out var _))
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
            var state = agent.GetState();

            switch (state)
            {
                case AgentAIState.SUPPRESS_CREATURE:
                case AgentAIState.SUPPRESS_WORKER:
                    Log.Info("taking damage");
                    if (agent.hp < 0.25 * agent.maxHp || agent.mental < 0.25 * agent.maxMental)
                    {
                        agent.ReturnToSefira();
                    }
                    return;
            }
        }

        /// <summary>
        /// Clear all macros.
        /// </summary>
        public void Clear()
        {
            MacroCreatures.Clear();
            FarmingCreatures.ToList().ForEach(x => CancelFarm(x.Unit.room));
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
                    else if (macro.ForGift && agent.HasGift(macro.Creature, out var gift))
                    {
                        agent.ResetWaitingPassage();
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

        public void ManageCreatures()
        {
            if (!Running)
            {
                return;
            }

            // handle Qliphoth meltdown events for kits
            foreach (var kit in CreatureManager.instance.GetCreatureList().Where(x => x.IsKit()))
            {
                HandleKitEvents(kit);
            }

            // handle Qliphoth meltdown events and other ugent events for creatures.
            if (HandleCreatureUrgentEvents())
            {
                return;
            }

            // auto suppress escaped creatures (only a few of them)
            SuppressEscapedCreatures();

            // auto suppress ordeal creatures.
            foreach (var creature in OrdealManager.instance.GetOrdealCreatureList())
            {
                SuppressOrdealCreature(creature);
            }

            // parse macro / farm when handling ordeals.
            if (InEmergency)
            {
                AgentManager.instance.GetAgentList().ToList().ForEach(x => x.SetWaitingPassage());
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

        private bool HandleCreatureUrgentEvents()
        {
            var agents = AgentManager.instance.GetAgentList().Where(x => x.IsAvailable());
            var creatures = CreatureManager.instance.GetCreatureList().FilterUrgent();
            var candidates = Candidate.Suggest(agents, creatures);
            candidates.Sort(Candidate.ManageComparer);

            int count = 0;
            foreach (var candidate in candidates)
            {
                if (candidate.Agent.IsAvailable() && candidate.Creature.IsAvailable())
                {
                    candidate.Apply();
                    count++;
                }
            }

            foreach (var creature in CreatureManager.instance.GetCreatureList().FilterUrgent())
            {
                Log.Info($"Cannot find candidates for {creature.metaInfo.name}");
            }
            return count > 0;
        }

        private void HandleKitEvents(CreatureModel kit)
        {
            var ext = kit.GetKitExtension();
            if (kit.isOverloaded && kit.IsAvailable())
            {
                ext.Handle();
            }
            ext.OnFixedUpdate();
        }

        private void SuppressEscapedCreatures()
        {
            foreach (var creature in CreatureManager.instance.GetCreatureList().Where(x => x.state == CreatureState.ESCAPE))
            {
                Log.Debug($"{creature.metaInfo.name} escaped.");
                if (creature.GetExtension().AutoSuppress)
                {
                    creature.FindAgents(100).FilterCanSuppress(creature).ToList().ForEach(x => x.Suppress(creature));
                }
            }
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
                    var agents = creature.FindAgents(80).FilterCanSuppress(creature).ToList();
                    if (agents.Count > 0)
                    {
                        agents.ForEach(x => x.Suppress(creature));
                    }
                    else
                    {
                        agents = creature.FindAgents(200).FilterCanSuppress(creature).ToList();
                        agents.ForEach(x => x.Suppress(creature));
                    }
                    return;
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