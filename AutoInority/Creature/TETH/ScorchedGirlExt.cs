namespace AutoInority.Creature
{
    internal class ScorchedGirlExt : ExpectGoodAndNormalExt
    {
        protected override SkillTypeInfo[] NormalSkillSets { get; } = { Instinct, Insight, Repression };

        public ScorchedGirlExt(CreatureModel creature) : base(creature)
        {
        }
    }
}