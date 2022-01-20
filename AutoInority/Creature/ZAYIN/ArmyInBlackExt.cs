namespace AutoInority.Creature
{
    internal class ArmyInBlackExt : GoodNormalExt
    {
        public override SkillTypeInfo[] SkillSets => new SkillTypeInfo[] { Insight };

        public ArmyInBlackExt(CreatureModel creature) : base(creature)
        {
        }
    }
}