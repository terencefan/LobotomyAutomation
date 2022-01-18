namespace AutoInority.Creature
{
    internal class CloudedMonkExt : GoodNormalExt
    {
        protected override SkillTypeInfo[] DefaultSkills { get; } = new SkillTypeInfo[] { Insight, Attachment, Repression };

        public CloudedMonkExt(CreatureModel creature) : base(creature)
        {
        }
    }
}