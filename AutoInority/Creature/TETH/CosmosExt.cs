namespace AutoInority.Creature
{
    internal class CosmosExt : ExpectGoodAndNormalExt
    {
        protected override SkillTypeInfo[] NormalSkillSets { get; } = { Attachment, Repression };

        public CosmosExt(CreatureModel creature) : base(creature)
        {
        }
    }
}