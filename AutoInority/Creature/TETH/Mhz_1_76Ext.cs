namespace AutoInority.Creature.TETH
{
    internal class Mhz_1_76Ext : ExpectGoodAndNormalExt
    {
        private static SkillTypeInfo[] TypeNormal { get; } = { Instinct, Insight, Repression };

        private static SkillTypeInfo[] TypeGood { get; } = { Repression };

        public override SkillTypeInfo[] SkillSets => QliphothCounter == 1 ? TypeGood : TypeNormal;

        public Mhz_1_76Ext(CreatureModel creature) : base(creature)
        {
        }
    }
}
