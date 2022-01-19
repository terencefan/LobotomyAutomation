namespace AutoInority.Creature
{
    internal class MurdererExt : GoodNormalExt
    {
        public MurdererExt(CreatureModel creature) : base(creature)
        {
        }

        protected override SkillTypeInfo[] DefaultSkills => new SkillTypeInfo[] { Instinct, Attachment };
    }
}