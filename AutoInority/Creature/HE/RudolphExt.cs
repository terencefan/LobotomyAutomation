namespace AutoInority.Creature
{
    internal class RudolphExt : ExpectGoodAndNormalExt
    {
        protected override SkillTypeInfo[] NormalSkillSets { get; } = { Insight, Attachment };

        public RudolphExt(CreatureModel creature) : base(creature)
        {
        }
    }
}
