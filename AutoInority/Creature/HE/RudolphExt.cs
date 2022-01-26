namespace AutoInority.Creature
{
    internal class RudolphExt : ExpectGoodAndNormalExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = { Insight, Attachment };

        public RudolphExt(CreatureModel creature) : base(creature)
        {
        }
    }
}
