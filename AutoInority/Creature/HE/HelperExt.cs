namespace AutoInority.Creature
{
    internal class HelperExt : ExpectGoodAndNormalExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Instinct, Repression };

        public HelperExt(CreatureModel creature) : base(creature)
        {
        }
    }
}
