namespace AutoInority.Creature
{
    internal class ExpectGoodAndNormalExt : BaseCreatureExt
    {
        public ExpectGoodAndNormalExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CheckConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            return CheckGoodNormal(agent, skill) && base.CheckConfidence(agent, skill);
        }

        protected virtual bool CheckGoodNormal(AgentModel agent, SkillTypeInfo skill)
        {
            if (QliphothCounter > 1)
            {
                return NormalConfidence(agent, skill) > Automaton.Instance.CounterDecreaseConfidence;
            }
            return IsOverloaded || GoodConfidence(agent, skill) > Automaton.Instance.CreatureEscapeConfidence;
        }
    }
}