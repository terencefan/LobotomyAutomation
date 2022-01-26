namespace AutoInority.Creature
{
    internal class CosmosExt : ExpectGoodAndNormalExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Attachment, Repression };

        public CosmosExt(CreatureModel creature) : base(creature)
        {
        }
    }
}
