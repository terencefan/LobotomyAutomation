namespace AutoInority.Creature
{
    internal class GoodNormalExt : BaseCreatureExt
    {
        public GoodNormalExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CheckConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            return CheckGoodNormal(agent, skill) && base.CheckConfidence(agent, skill);
        }
    }
}