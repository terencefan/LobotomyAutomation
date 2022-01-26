namespace AutoInority.Creature.TETH
{
    internal class Mhz_1_76Ext : ExpectGoodAndNormalExt
    {
        protected override SkillTypeInfo[] NormalSkillSets { get; } = { Instinct, Insight, Repression };

        protected override SkillTypeInfo[] GoodSkillSets { get; } = { Repression };

        public Mhz_1_76Ext(CreatureModel creature) : base(creature)
        {
        }
    }
}
