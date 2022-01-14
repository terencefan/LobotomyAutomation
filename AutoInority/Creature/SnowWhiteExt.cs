namespace AutoInority.Creature
{
    internal class SnowWhiteExt : GoodNormalExt
    {
        protected override SkillTypeInfo[] DefaultSkills { get; } = new SkillTypeInfo[] { Instinct, Insight, Repression };

        public SnowWhiteExt(CreatureModel creature) : base(creature)
        {
        }
    }
}