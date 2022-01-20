namespace AutoInority.Creature
{
    internal class CloudedMonkExt : ExpectGoodAndNormalExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Insight, Attachment, Repression };

        public CloudedMonkExt(CreatureModel creature) : base(creature)
        {
        }
    }
}