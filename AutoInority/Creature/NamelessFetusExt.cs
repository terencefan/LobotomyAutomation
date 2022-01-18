namespace AutoInority.Creature
{
    class NamelessFetusExt : GoodNormalExt
    {
        public NamelessFetusExt(CreatureModel creature) : base(creature)
        {
        }

        protected override SkillTypeInfo[] DefaultSkills => new SkillTypeInfo[] { Instinct };
    }
}
