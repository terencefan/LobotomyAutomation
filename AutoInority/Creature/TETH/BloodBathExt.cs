namespace AutoInority.Creature
{
    internal class BloodBathExt : BaseCreatureExt
    {
        public BloodBathExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (agent.fortitudeLevel == 1 || agent.temperanceLevel == 1)
            {
                message = Message(Angela.Creature.BloodBath, agent, skill);
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }
    }
}