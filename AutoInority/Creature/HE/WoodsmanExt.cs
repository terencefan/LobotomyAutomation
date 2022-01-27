namespace AutoInority.Creature
{
    internal class WoodsmanExt : ExpectNormalExt
    {
        public WoodsmanExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (QliphothCounter == 0)
            {
                message = Message(Angela.Creature.Woodsman, agent, skill);
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }

        public override bool CheckConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            if (agent.prudenceLevel > 2)
            {
                if (IsOverloaded && QliphothCounter > 0)
                {
                    return base.CheckConfidence(agent, skill);
                }
                return false;
            }
            return base.CheckConfidence(agent, skill);
        }
    }
}