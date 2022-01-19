namespace AutoInority.Creature
{
    internal class OneBadManyGoodExt : BaseCreatureExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Insight, Attachment };

        public OneBadManyGoodExt(CreatureModel creature) : base(creature)
        {
        }
    }
}