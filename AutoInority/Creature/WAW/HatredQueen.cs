namespace AutoInority.Creature
{
    internal class HatredQueen : GoodNormalExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Instinct, Insight, Attachment };

        public override bool IsUrgent => _creature.qliphothCounter == 1;

        public HatredQueen(CreatureModel creature) : base(creature)
        {
        }
    }
}