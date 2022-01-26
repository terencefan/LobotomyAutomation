namespace AutoInority.Creature
{
    internal class SnowWhiteAppleExt : ExpectGoodAndNormalExt
    {
        protected override SkillTypeInfo[] NormalSkillSets { get; } = { Instinct, Insight, Repression };

        protected override SkillTypeInfo[] GoodSkillSets { get; } = { Repression };

        public SnowWhiteAppleExt(CreatureModel creature) : base(creature)
        {
        }
    }
}