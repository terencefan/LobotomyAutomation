using System;
using System.Collections.Generic;

using AutoInority.Extentions;

namespace AutoInority
{
    public sealed class Candidate
    {
        public AgentModel Agent { get; private set; }

        public CreatureModel Creature { get; private set; }

        public float GoodConfidence { get; private set; }

        public bool HasEGOGift { get; private set; }

        public bool HasReachedExpLimit { get; private set; }

        public SkillTypeInfo Skill { get; private set; }

        public Candidate(AgentModel agent, CreatureModel creature, SkillTypeInfo skill)
        {
            Agent = agent;
            Creature = creature;
            Skill = skill;
            GoodConfidence = creature.GetExtension().GoodConfidence(agent, skill);
            HasEGOGift = agent.HasEGOGift(creature, out _);
            HasReachedExpLimit = agent.HasReachedExpLimit(skill.rwbpType, out _);
        }

        public static int Comparer(Candidate x, Candidate y)
        {
            if (!x.HasEGOGift && x.GoodConfidence > 0.9f)
            {
                return -1;
            }
            else if (!y.HasEGOGift && y.GoodConfidence > 0.9f)
            {
                return 1;
            }
            else if (!x.HasReachedExpLimit && x.GoodConfidence > 0.9f)
            {
                return -1;
            }
            else if (!y.HasReachedExpLimit && y.GoodConfidence > 0.9f)
            {
                return 1;
            }
            else
            {
                return y.GoodConfidence.CompareTo(x.GoodConfidence);
            }
        }

        public static List<Candidate> Suggest(IEnumerable<AgentModel> agents, IEnumerable<CreatureModel> creatures)
        {
            var candidates = new List<Candidate>();
            foreach (var creature in creatures)
            {
                var ext = creature.GetExtension();
                foreach (var agent in agents)
                {
                    foreach (var skill in ext.SkillSets)
                    {
                        if (ext.CanWorkWith(agent, skill, out _) && ext.CheckConfidence(agent, skill))
                        {
                            candidates.Add(new Candidate(agent, creature, skill));
                        }
                    }
                }
            }
            candidates.Sort(Comparer);
            return candidates;
        }

        public static List<Candidate> Suggest(AgentModel agent, IEnumerable<CreatureModel> creatures) => Suggest(new[] { agent }, creatures);

        public static List<Candidate> Suggest(IEnumerable<AgentModel> agents, CreatureModel creature) => Suggest(agents, new[] { creature });

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

        public override string ToString() => $"{Agent.name} - {Creature.metaInfo.name} - {Skill.calledName}: {GoodConfidence}";
    }
}