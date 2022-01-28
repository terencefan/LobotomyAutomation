namespace AutoInority.Creature
{
    internal class QueenBeeExt : ExpectGoodAndNormalExt
    {
        protected override SkillTypeInfo[] NormalSkillSets { get; } = { Insight };

        public QueenBeeExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CheckWorkConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            if (IsFarming && NormalConfidence(agent, skill) > CreatureEscapeConfidence)
            {
                return true;
            }
            return base.CheckWorkConfidence(agent, skill);
        }
    }
}