namespace AutoInority.Creature
{
    internal class BigBirdExt : ExpectGoodAndNormalExt
    {
        public override bool IsUrgent => QliphothCounter < 5;

        protected override SkillTypeInfo[] NormalSkillSets { get; } = { Instinct, Attachment };

        public BigBirdExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CheckConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            if (QliphothCounter < 3 && NormalConfidence(agent, skill) < CreatureEscapeConfidence)
            {
                return false;
            }
            return base.CheckConfidence(agent, skill);
        }
    }
}