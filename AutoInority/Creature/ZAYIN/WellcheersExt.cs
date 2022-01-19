namespace AutoInority.Creature
{
    internal class WellcheersExt : BaseCreatureExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Instinct, Insight };

        public WellcheersExt(CreatureModel creature) : base(creature)
        {
        }
    }
}