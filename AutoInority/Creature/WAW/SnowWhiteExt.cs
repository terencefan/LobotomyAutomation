namespace AutoInority.Creature
{
    internal class SnowWhiteExt : ExpectGoodAndNormalExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Instinct, Insight, Repression };

        public SnowWhiteExt(CreatureModel creature) : base(creature)
        {
        }
    }
}