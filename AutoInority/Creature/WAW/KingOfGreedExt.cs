namespace AutoInority.Creature
{
    internal class KingOfGreedExt : ExpectGoodAndNormalExt
    {
        protected override SkillTypeInfo[] NormalSkillSets { get; } = { Instinct, Attachment, Repression };

        public KingOfGreedExt(CreatureModel creature) : base(creature)
        {
        }
    }
}
