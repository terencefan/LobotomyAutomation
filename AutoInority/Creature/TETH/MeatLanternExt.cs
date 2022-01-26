using AutoInority.Extentions;

namespace AutoInority.Creature
{
    internal class MeatLanternExt : BaseCreatureExt
    {
        public MeatLanternExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (_creature.CalculateWorkTime(agent) < 41)
            {
                message = "";
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }

        public override bool CheckConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            if (QliphothCounter > 1)
            {
                return NormalConfidence(agent, skill) > Automaton.Instance.CounterDecreaseConfidence && base.CheckConfidence(agent, skill);
            }
            else
            {
                return NormalConfidence(agent, skill) > Automaton.Instance.CreatureEscapeConfidence && base.CheckConfidence(agent, skill);
            }
        }
    }
}