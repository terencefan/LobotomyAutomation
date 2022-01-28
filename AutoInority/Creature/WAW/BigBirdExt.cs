namespace AutoInority.Creature
{
    internal class BigBirdExt : ExpectGoodAndNormalExt
    {
        public override bool IsUrgent => QliphothCounter < 5;

        protected override SkillTypeInfo[] NormalSkillSets { get; } = { Instinct, Attachment };

        public BigBirdExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CheckWorkConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            return IsUrgent ? GoodConfidence(agent, skill) > 0.8f : base.CheckWorkConfidence(agent, skill);
        }
    }
}