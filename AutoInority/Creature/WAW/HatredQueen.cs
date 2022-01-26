namespace AutoInority.Creature
{
    internal class HatredQueen : ExpectGoodAndNormalExt
    {
        public override bool IsUrgent => QliphothCounter < 2;

        protected override SkillTypeInfo[] NormalSkillSets { get; } = { Instinct, Insight, Attachment };

        protected override SkillTypeInfo[] GoodSkillSets { get; } = { Attachment };

        public HatredQueen(CreatureModel creature) : base(creature)
        {
        }
    }
}