namespace AutoInority.Creature
{
    internal class NamelessFetusExt : ExpectGoodAndNormalExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Instinct };

        public NamelessFetusExt(CreatureModel creature) : base(creature)
        {
        }
    }
}