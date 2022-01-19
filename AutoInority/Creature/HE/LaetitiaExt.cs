namespace AutoInority.Creature
{
    internal class LaetitiaExt : BaseCreatureExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Instinct, Insight, Attachment };

        public LaetitiaExt(CreatureModel creature) : base(creature)
        {
        }
    }
}