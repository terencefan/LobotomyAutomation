namespace AutoInority.Creature
{
    internal class QueenBeeExt : GoodNormalExt
    {
        protected override SkillTypeInfo[] DefaultSkills { get; } = new SkillTypeInfo[] { Instinct, Insight, Attachment };

        public QueenBeeExt(CreatureModel creature) : base(creature)
        {
        }
    }
}