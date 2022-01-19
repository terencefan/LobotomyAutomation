namespace AutoInority.Creature
{
    internal class WellcheersExt : BaseCreatureExt
    {
        public WellcheersExt(CreatureModel creature) : base(creature)
        {
        }

        protected override SkillTypeInfo[] DefaultSkills => new SkillTypeInfo[] { Instinct, Insight };
    }
}