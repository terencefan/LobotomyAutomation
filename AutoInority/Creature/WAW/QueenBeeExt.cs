namespace AutoInority.Creature
{
    internal class QueenBeeExt : ExpectGoodAndNormalExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Instinct, Insight, Attachment };

        public QueenBeeExt(CreatureModel creature) : base(creature)
        {
        }
    }
}