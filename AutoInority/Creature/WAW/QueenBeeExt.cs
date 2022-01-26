namespace AutoInority.Creature
{
    internal class QueenBeeExt : ExpectGoodAndNormalExt
    {
        protected override SkillTypeInfo[] NormalSkillSets { get; } = { Insight };

        public QueenBeeExt(CreatureModel creature) : base(creature)
        {
        }
    }
}