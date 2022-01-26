namespace AutoInority.Creature
{
    internal class PinkCorpsExt : ExpectNormalExt
    {
        public override SkillTypeInfo[] SkillSets => new SkillTypeInfo[] { Insight };

        public PinkCorpsExt(CreatureModel creature) : base(creature)
        {
        }
    }
}