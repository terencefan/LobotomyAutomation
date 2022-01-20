namespace AutoInority.Creature
{
    internal class JudgementBirdExt : ExpectGoodAndNormalExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Instinct, Insight, Attachment };

        public JudgementBirdExt(CreatureModel creature) : base(creature)
        {
        }
    }
}