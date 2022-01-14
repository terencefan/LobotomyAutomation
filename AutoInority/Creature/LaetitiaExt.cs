namespace AutoInority.Creature
{
    internal class LaetitiaExt : BaseCreatureExt
    {
        protected override SkillTypeInfo[] DefaultSkills { get; } = new SkillTypeInfo[] { Instinct, Insight, Attachment };

        public LaetitiaExt(CreatureModel creature) : base(creature)
        {
        }
    }
}