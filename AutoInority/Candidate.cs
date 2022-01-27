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

        public float GoodConfidence { get; private set; }

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
            GoodConfidence = creature.GetExtension().GoodConfidence(agent, skill);
            HasGift = agent.HasGift(creature, out var gift);
            HasAnotherGift = agent.HasAnotherGift(gift);
            HasReachedExpLimit = agent.HasReachedExpLimit(skill.rwbpType, out _);

            var ext = creature.GetExtension();
            ManageConfidence = GoodConfidence - 0.002 * Distance - ext.ConfidencePenalty(agent, skill);
            FarmConfidence = GoodConfidence - 0.001 * Distance - (HasReachedExpLimit ? 0.2 : 0) - (HasGift || HasAnotherGift ? 0.5 : 0) - ext.ConfidencePenalty(agent, skill);
        }

        public static int FarmComparer(Candidate x, Candidate y) => y.FarmConfidence.CompareTo(x.FarmConfidence);

        public static int ManageComparer(Candidate x, Candidate y) => y.ManageConfidence.CompareTo(x.ManageConfidence);

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
                        if (skill == null)
                        {
                            Log.Debug($"creature: {creature.metaInfo.name}, skill is null, length: {skillSets.Count()}, origin: {ext.SkillSets.Count()}");
                        }
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

        public override string ToString() => $"{Agent.name}: {GoodConfidence}, Distance: {Distance}, HasGift: {HasGift}, HasAnotherGift: {HasAnotherGift}, Exp: {HasReachedExpLimit}";
    }
}