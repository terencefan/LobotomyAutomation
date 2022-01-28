namespace AutoInority.Creature
{
    internal class SmilingBodyMountainExt : ExpectNormalExt
    {
        public SmilingBodyMountainExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (agent.hp < agent.maxHp)
            {
                message = Message(Angela.Creature.BodyMountain, agent, skill);
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }
    }
}
