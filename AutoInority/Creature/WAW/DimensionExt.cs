namespace AutoInority.Creature
{
    internal class DimensionExt : BaseCreatureExt
    {
        public DimensionExt(CreatureModel creature) : base(creature)
        {
            // TODO check work time
        }

        public override bool CheckConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            var confidence = NormalConfidence(agent, skill);
            if (QliphothCounter > 1)
            {
                if (confidence < Automaton.Instance.CounterDecreaseConfidence)
                {
                    return false;
                }
            }
            else
            {
                if (confidence < Automaton.Instance.CreatureEscapeConfidence)
                {
                    return false;
                }
            }
            return base.CheckConfidence(agent, skill);
        }
    }
}