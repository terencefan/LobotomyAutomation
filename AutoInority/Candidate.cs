using System;
using System.Collections.Generic;

using AutoInority.Extentions;

namespace AutoInority
{
    internal sealed class Candidate
    {
        public AgentModel Agent { get; private set; }

        public CreatureModel Creature { get; private set; }

        public float GoodConfidence { get; private set; }

        public SkillTypeInfo Skill { get; private set; }

        public Candidate(AgentModel agent, CreatureModel creature, SkillTypeInfo skill)
        {
            Agent = agent;
            Creature = creature;
            Skill = skill;
            GoodConfidence = creature.GetCreatureExtension().GoodConfidence(agent, skill);
        }

        public static int Comparer(Candidate x, Candidate y)
        {
            var xExp = x.Agent.HasReachedExpLimit(x.Skill.rwbpType, out _);
            var yExp = y.Agent.HasReachedExpLimit(y.Skill.rwbpType, out _);
            if (xExp && !yExp && x.GoodConfidence > 0.9f)
            {
                return -1;
            }
            else if (yExp && !xExp && y.GoodConfidence > 0.9f)
            {
                return -1;
            }
            return y.GoodConfidence.CompareTo(x.GoodConfidence);
        }

        public static List<Candidate> Suggest(IEnumerable<AgentModel> agents, IEnumerable<CreatureModel> creatures)
        {
            var candidates = new List<Candidate>();
            foreach (var creature in creatures)
            {
                var ext = creature.GetCreatureExtension();
                foreach (var agent in agents)
                {
                    foreach (var skill in ext.SkillSets())
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
                Log.Error(e.Message);
                Log.Error(e.StackTrace);
            }
        }
        public override string ToString() => $"{Agent.name} - {Creature.metaInfo.name} - {Skill.calledName}: {GoodConfidence}";
    }
}