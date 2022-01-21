namespace AutoInority.Creature
{
    internal class PinkCorpsExt : ExpectGoodAndNormalExt
    {
        public override SkillTypeInfo[] SkillSets => new SkillTypeInfo[] { Insight };

        public PinkCorpsExt(CreatureModel creature) : base(creature)
        {
        }
    }
}