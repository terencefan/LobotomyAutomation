namespace AutoInority.Creature
{
    internal class CloudedMonkExt : ExpectGoodAndNormalExt
    {
        protected override SkillTypeInfo[] NormalSkillSets { get; } = { Insight, Attachment, Repression };

        public CloudedMonkExt(CreatureModel creature) : base(creature)
        {
        }
    }
}