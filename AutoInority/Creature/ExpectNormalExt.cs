namespace AutoInority.Creature
{
    internal class ExpectNormalExt : BaseCreatureExt
    {
        public ExpectNormalExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CheckConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            var confidence = NormalConfidence(agent, skill);
            if (QliphothCounter > 1 && confidence < Automaton.Instance.CounterDecreaseConfidence)
            {
                return false;
            }
            else if (confidence < CreatureEscapeConfidence)
            {
                return false;
            }
            return base.CheckConfidence(agent, skill);
        }
    }
}
