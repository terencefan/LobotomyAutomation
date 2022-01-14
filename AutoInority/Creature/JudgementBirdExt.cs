namespace AutoInority.Creature
{
    internal class JudgementBirdExt : GoodNormalExt
    {
        protected override SkillTypeInfo[] DefaultSkills { get; } = new SkillTypeInfo[] { Instinct, Insight, Attachment };

        public JudgementBirdExt(CreatureModel creature) : base(creature)
        {
        }
    }
}