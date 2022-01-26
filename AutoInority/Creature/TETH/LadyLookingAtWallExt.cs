namespace AutoInority.Creature
{
    internal class LadyLookingAtWallExt : ExpectGoodAndNormalExt
    {
        protected override SkillTypeInfo[] NormalSkillSets { get; } = { Instinct, Repression };

        public LadyLookingAtWallExt(CreatureModel creature) : base(creature)
        {
        }
    }
}