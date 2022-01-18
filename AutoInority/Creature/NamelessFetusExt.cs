namespace AutoInority.Creature
{
    internal class NamelessFetusExt : GoodNormalExt
    {
        protected override SkillTypeInfo[] DefaultSkills => new SkillTypeInfo[] { Instinct };

        public NamelessFetusExt(CreatureModel creature) : base(creature)
        {
        }
    }
}
