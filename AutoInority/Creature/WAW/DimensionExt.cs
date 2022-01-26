namespace AutoInority.Creature
{
    internal class DimensionExt : ExpectNormalExt
    {
        public DimensionExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CheckConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            // TODO check work time
            return base.CheckConfidence(agent, skill);
        }
    }
}