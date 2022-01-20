namespace AutoInority.Creature
{
    internal class KingOfGreedExt : GoodNormalExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Instinct, Attachment, Repression };

        public KingOfGreedExt(CreatureModel creature) : base(creature)
        {
        }
    }
}
