namespace AutoInority.Creature
{
    internal class HelperExt : ExpectGoodAndNormalExt
    {
        protected override SkillTypeInfo[] NormalSkillSets { get; } = new SkillTypeInfo[] { Instinct, Repression };

        public HelperExt(CreatureModel creature) : base(creature)
        {
        }
    }
}