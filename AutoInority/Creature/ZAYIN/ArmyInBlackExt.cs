namespace AutoInority.Creature
{
    internal class ArmyInBlackExt : ExpectNormalExt
    {
        public override SkillTypeInfo[] SkillSets => new SkillTypeInfo[] { Insight };

        public ArmyInBlackExt(CreatureModel creature) : base(creature)
        {
        }
    }
}