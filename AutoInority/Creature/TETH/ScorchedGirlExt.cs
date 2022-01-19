namespace AutoInority.Creature
{
    internal class ScorchedGirlExt : GoodNormalExt
    {
        public override SkillTypeInfo[] SkillSets => new SkillTypeInfo[] { Instinct, Insight, Repression };

        public ScorchedGirlExt(CreatureModel creature) : base(creature)
        {
        }
    }
}