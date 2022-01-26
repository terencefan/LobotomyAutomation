namespace AutoInority.Creature
{
    internal class BloodyTreeExt : ExpectNormalExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Insight, Attachment, Repression };

        public BloodyTreeExt(CreatureModel creature) : base(creature)
        {
        }
    }
}