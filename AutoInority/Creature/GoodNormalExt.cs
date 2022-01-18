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

        protected virtual bool CheckGoodNormal(AgentModel agent, SkillTypeInfo skill)
        {
            if (_creature.qliphothCounter > 1)
            {
                return NormalConfidence(agent, skill) > Automaton.Instance.CounterDecreaseConfidence;
            }
            return GoodConfidence(agent, skill) > Automaton.Instance.CreatureEscapeConfidence;
        }
    }
}