namespace AutoInority.Creature
{
    internal class BlackSwanExt : GoodNormalExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Instinct, Insight, Repression };

        public BlackSwanExt(CreatureModel creature) : base(creature)
        {
        }
    }
}