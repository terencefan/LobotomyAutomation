namespace AutoInority.Creature
{
    internal class JudgementBirdExt : ExpectGoodAndNormalExt
    {
        protected override SkillTypeInfo[] NormalSkillSets { get; } = { Instinct, Insight, Attachment };

        public JudgementBirdExt(CreatureModel creature) : base(creature)
        {
        }
    }
}