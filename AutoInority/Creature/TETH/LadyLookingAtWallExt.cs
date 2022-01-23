namespace AutoInority.Creature
{
    internal class LadyLookingAtWallExt : ExpectGoodAndNormalExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Instinct, Repression };

        public LadyLookingAtWallExt(CreatureModel creature) : base(creature)
        {
        }
    }
}