namespace AutoInority.Creature
{
    internal class ShyLook : BaseCreatureExt
    {
        public ShyLook(CreatureModel creature) : base(creature)
        {
        }

        protected override float CalculateWorkSuccessProb(AgentModel agent, SkillTypeInfo skill)
        {
            return base.CalculateWorkSuccessProb(agent, skill) - 0.5f;
        }

        protected override float GetDamageMultiplierInWork(AgentModel agent, SkillTypeInfo skill)
        {
            return 2f;
        }
    }
}