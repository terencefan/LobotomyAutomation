namespace AutoInority.Creature
{
    internal class BlueStarExt : BaseCreatureExt
    {
        public BlueStarExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (agent.temperanceLevel < 4)
            {
                message = Message(Angela.Creature.BlueStarDie, agent, skill);
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }

        public override bool CheckConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            if (agent.prudenceLevel < 5)
            {
                return false;
            }
            return base.CheckConfidence(agent, skill);
        }
    }
}