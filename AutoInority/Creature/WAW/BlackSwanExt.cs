namespace AutoInority.Creature
{
    internal class BlackSwanExt : ExpectGoodAndNormalExt
    {
        protected override SkillTypeInfo[] NormalSkillSets { get; } = { Instinct, Insight, Repression };

        public BlackSwanExt(CreatureModel creature) : base(creature)
        {
        }
    }
}