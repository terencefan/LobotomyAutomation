namespace AutoInority.Creature
{
    internal class RedHoodExt : BaseCreatureExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = { Instinct, Insight };

        public RedHoodExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (agent.GetRecentWorkedCreature()?.script is BigBadWolf)
            {
                message = Message(Angela.Creature.RedHood, agent, skill);
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }
    }
}
