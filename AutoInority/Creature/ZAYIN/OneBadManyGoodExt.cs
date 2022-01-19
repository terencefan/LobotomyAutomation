namespace AutoInority.Creature
{
    internal class OneBadManyGoodExt : BaseCreatureExt
    {
        public OneBadManyGoodExt(CreatureModel creature) : base(creature)
        {
        }

        protected override SkillTypeInfo[] DefaultSkills => new SkillTypeInfo[] { Insight, Attachment };
    }
}