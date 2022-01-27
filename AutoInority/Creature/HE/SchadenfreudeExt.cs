namespace AutoInority.Creature
{
    internal class SchadenfreudeExt : BaseCreatureExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = { Insight, Repression };

        public SchadenfreudeExt(CreatureModel creature) : base(creature)
        {
        }
    }
}