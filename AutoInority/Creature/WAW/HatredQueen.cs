namespace AutoInority.Creature
{
    internal class HatredQueen : ExpectGoodAndNormalExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Instinct, Insight, Attachment };

        public override bool IsUrgent => QliphothCounter < 2;

        public HatredQueen(CreatureModel creature) : base(creature)
        {
        }
    }
}