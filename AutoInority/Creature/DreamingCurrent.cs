namespace AutoInority.Creature
{
    class DreamingCurrent : BaseCreatureExt
    {
        public DreamingCurrent(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (agent.temperanceLevel == 1)
            {
                message = "";
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }
    }
}
