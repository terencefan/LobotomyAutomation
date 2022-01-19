namespace AutoInority.Creature
{
    internal class MurdererExt : GoodNormalExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Instinct, Attachment };

        public MurdererExt(CreatureModel creature) : base(creature)
        {
        }
    }
}