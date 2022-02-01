namespace AutoInority.Creature.WAW
{
    internal class LunaExt : ExpectGoodAndNormalExt
    {
        protected override SkillTypeInfo[] GoodSkillSets { get; } = { Instinct, Attachment };

        protected override SkillTypeInfo[] NormalSkillSets { get; } = { Instinct, Attachment, Repression };

        public LunaExt(CreatureModel creature) : base(creature)
        {
        }
    }
}