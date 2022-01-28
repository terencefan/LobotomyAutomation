namespace AutoInority.Creature
{
    internal class DimensionExt : ExpectNormalExt
    {
        public DimensionExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CheckWorkConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            return CalculateWorkTime(agent) < 40 && base.CheckWorkConfidence(agent, skill);
        }
    }
}