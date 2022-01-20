namespace AutoInority.Creature
{
    internal class PinkCorpsExt : GoodNormalExt
    {
        public override SkillTypeInfo[] SkillSets => new SkillTypeInfo[] { Insight };

        public PinkCorpsExt(CreatureModel creature) : base(creature)
        {
        }
    }
}