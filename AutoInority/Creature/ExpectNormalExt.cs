namespace AutoInority.Creature
{
    internal class ExpectNormalExt : BaseCreatureExt
    {
        public ExpectNormalExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CheckWorkConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            var confidence = NormalConfidence(agent, skill);
            if (QliphothCounter > 1 && confidence > Automaton.Instance.CounterDecreaseConfidence)
            {
                return true;
            }
            else if (confidence > CreatureEscapeConfidence)
            {
                return true;
            }
            return false;
        }
    }
}