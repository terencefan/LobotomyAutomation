using System;
using System.Collections.Generic;
using System.Linq;

using AutoInority.Extentions;

namespace AutoInority
{
    public sealed class Candidate
    {
        public AgentModel Agent { get; private set; }

        public CreatureModel Creature { get; private set; }

        public float Distance { get; private set; }

        public double FarmConfidence { get; private set; }

        public bool HasAnotherGift { get; private set; }

        public bool HasGift { get; private set; }

        public bool HasReachedExpLimit { get; private set; }

        public double ManageConfidence { get; private set; }

        public SkillTypeInfo Skill { get; private set; }

        public Candidate(AgentModel agent, CreatureModel creature, SkillTypeInfo skill)
        {
            Agent = agent;
            Creature = creature;
            Skill = skill;
            Distance = Graph.Distance(agent, creature);
            HasGift = agent.HasGift(creature, out var gift);
            HasAnotherGift = agent.HasAnotherGift(gift);
            HasReachedExpLimit = agent.HasReachedExpLimit(skill.rwbpType, out _);

            var ext = creature.GetExtension();
            var confidence = ext.GetConfidence(agent, skill);
            ManageConfidence = confidence * (1 - 0.002 * Distance) * ext.ConfidenceMultiplifier(agent, skill);
            FarmConfidence = confidence * (1 - 0.001 * Distance) * (HasReachedExpLimit ? 0.8 : 1) * (HasGift || HasAnotherGift ? 0.5 : 1) * ext.ConfidenceMultiplifier(agent, skill);
        }

        public static int FarmComparer(Candidate x, Candidate y) => y.FarmConfidence.CompareTo(x.FarmConfidence);

        public static int ManageComparer(Candidate x, Candidate y)
        {
            var mx = x.ManageConfidence + x.Creature.GetRiskLevel();
            var my = x.ManageConfidence + y.Creature.GetRiskLevel();
            return my.CompareTo(mx);
        }

        public static List<Candidate> Suggest(IEnumerable<AgentModel> agents, IEnumerable<CreatureModel> creatures, HashSet<RwbpType> types = null)
        {
            var candidates = new List<Candidate>();
            foreach (var creature in creatures)
            {
                var ext = creature.GetExtension();
                var skillSets = ext.SkillSets.Where(x => types == null || types.Contains(x.rwbpType));

                foreach (var agent in agents)
                {
                    foreach (var skill in skillSets)
                    {
                        var b1 = ext.CanWorkWith(agent, skill, out _);
                        var b2 = ext.CheckConfidence(agent, skill);
                        Log.Debug($"{creature.metaInfo.name}, b1: {b1}, b2: {b2}");
                        if (b1 && b2)
                        {
                            candidates.Add(new Candidate(agent, creature, skill));
                        }
                    }
                }
            }
            return candidates;
        }

        public static List<Candidate> Suggest(AgentModel agent, IEnumerable<CreatureModel> creatures, HashSet<RwbpType> types = null) => Suggest(new[] { agent }, creatures, types);

        public static List<Candidate> Suggest(IEnumerable<AgentModel> agents, CreatureModel creature) => Suggest(agents, new[] { creature });

        public static int TrainComparer(AgentModel x, AgentModel y) => x.TotalStats().CompareTo(y.TotalStats());

        public void Apply()
        {
            try
            {
                var sprite = CommandWindow.CommandWindow.CurrentWindow.GetWorkSprite(Skill.rwbpType);
                Agent.ManageCreature(Creature, Skill, sprite);
                Agent.counterAttackEnabled = false;
                Creature.Unit.room.OnWorkAllocated(Agent);
                Creature.script.OnWorkAllocated(Skill, Agent);
                AngelaConversation.instance.MakeMessage(AngelaMessageState.MANAGE_START, Agent, Skill, Creature);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public bool IsAvailable() => Agent.IsAvailable() && Creature.IsAvailable();

        public override string ToString() => $"{Agent.name} ({FarmConfidence}, {ManageConfidence}), Distance: {Distance}, HasGift: {HasGift}, HasAnotherGift: {HasAnotherGift}, Exp: {HasReachedExpLimit}";
    }
}