namespace AutoInority.Creature
{
    internal class NamelessFetusExt : ExpectGoodAndNormalExt
    {
        protected override SkillTypeInfo[] NormalSkillSets { get; } = {Instinct };

        public NamelessFetusExt(CreatureModel creature) : base(creature)
        {
        }
    }
}