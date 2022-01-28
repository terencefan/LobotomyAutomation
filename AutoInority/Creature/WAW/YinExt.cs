namespace AutoInority.Creature
{
    internal class YinExt : ExpectNormalExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = { Instinct, Insight, Repression };

        public YinExt(CreatureModel creature) : base(creature)
        {
        }
    }
}
