namespace AutoInority.Creature
{
    internal class SnowWhiteAppleExt : ExpectGoodAndNormalExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Instinct, Insight, Repression };

        public SnowWhiteAppleExt(CreatureModel creature) : base(creature)
        {
        }
    }
}