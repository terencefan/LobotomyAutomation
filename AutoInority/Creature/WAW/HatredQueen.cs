namespace AutoInority.Creature
{
    internal class HatredQueen : GoodNormalExt
    {
        protected override SkillTypeInfo[] DefaultSkills { get; } = new SkillTypeInfo[] { Instinct, Insight, Attachment };

        public HatredQueen(CreatureModel creature) : base(creature)
        {
        }

        public override bool IsUrgent => _creature.qliphothCounter == 1;
    }
}