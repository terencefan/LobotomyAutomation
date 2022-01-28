namespace AutoInority.Creature
{
    internal class QueenBeeExt : ExpectGoodAndNormalExt
    {
        protected override SkillTypeInfo[] NormalSkillSets { get; } = { Insight };

        public QueenBeeExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CheckConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            if (IsFarming && NormalConfidence(agent, skill) > CreatureEscapeConfidence)
            {
                return CheckSurvive(agent, skill);
            }
            return base.CheckConfidence(agent, skill);
        }
    }
}