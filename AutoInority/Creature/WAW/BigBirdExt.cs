namespace AutoInority.Creature
{
    internal class BigBirdExt : ExpectGoodAndNormalExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Instinct, Attachment };

        public override bool IsUrgent => QliphothCounter < 5;

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