namespace AutoInority.Creature
{
    internal class ScorchedGirlExt : GoodNormalExt
    {
        protected override SkillTypeInfo[] DefaultSkills => new SkillTypeInfo[] { Instinct, Insight, Repression };

        public ScorchedGirlExt(CreatureModel creature) : base(creature)
        {
        }
    }
}