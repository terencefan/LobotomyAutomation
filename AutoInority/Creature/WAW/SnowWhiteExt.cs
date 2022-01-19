namespace AutoInority.Creature
{
    internal class SnowWhiteExt : GoodNormalExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Instinct, Insight, Repression };

        public SnowWhiteExt(CreatureModel creature) : base(creature)
        {
        }
    }
}