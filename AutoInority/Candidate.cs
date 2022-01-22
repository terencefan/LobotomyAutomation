using System;
using System.Collections.Generic;

using AutoInority.Extentions;

namespace AutoInority
{
    public sealed class Candidate
    {
        public AgentModel Agent { get; private set; }

        public CreatureModel Creature { get; private set; }

        public int SefiraDistance { get; private set; }

        public float Distance { get; private set; }

        public float GoodConfidence { get; private set; }

        public bool HasEGOGift { get; private set; }

        public bool HasReachedExpLimit { get; private set; }

        public SkillTypeInfo Skill { get; private set; }

        public Candidate(AgentModel agent, CreatureModel creature, SkillTypeInfo skill)
        {
            Agent = agent;
            Creature = creature;
            Skill = skill;
            SefiraDistance = Graph.Distance(agent.GetActualSefira(), creature.sefira);
            Distance = Graph.Distance(agent, creature);
            GoodConfidence = creature.GetExtension().GoodConfidence(agent, skill);
            HasEGOGift = agent.HasEGOGift(creature, out _);
            HasReachedExpLimit = agent.HasReachedExpLimit(skill.rwbpType, out _);
        }

        public static int Comparer(Candidate x, Candidate y)
        {
            var xConf = x.GoodConfidence - 0.05 * y.SefiraDistance - (x.HasReachedExpLimit ? 0.1 : 0) - (x.HasEGOGift ? 0.1 : 0);
            var yConf = y.GoodConfidence - 0.05 * y.SefiraDistance - (y.HasReachedExpLimit ? 0.1 : 0) - (y.HasEGOGift ? 0.1 : 0);
            return yConf.CompareTo(xConf);
        }

        public static int FarmComparer(Candidate x, Candidate y)
        {
            var xConf = x.GoodConfidence - 0.05 * y.SefiraDistance - (x.HasReachedExpLimit ? 0.1 : 0);
            var yConf = y.GoodConfidence - 0.05 * y.SefiraDistance - (y.HasReachedExpLimit ? 0.1 : 0);
            return yConf.CompareTo(xConf);
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

        public override string ToString() => $"{Agent.name} - {Creature.metaInfo.name}: {SefiraDistance}, {Distance}";
    }
}