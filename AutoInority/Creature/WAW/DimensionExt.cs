namespace AutoInority.Creature
{
    internal class DimensionExt : ExpectNormalExt
    {
        public DimensionExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CheckConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            if (CalculateWorkTime(agent) > 40)
            {
                return false;
            }
            return base.CheckConfidence(agent, skill);
        }
    }
}