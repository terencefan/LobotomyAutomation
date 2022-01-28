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

        public override bool CheckWorkConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            if (agent.prudenceLevel > 2)
            {
                return IsOverloaded;
            }
            return base.CheckWorkConfidence(agent, skill);
        }
    }
}