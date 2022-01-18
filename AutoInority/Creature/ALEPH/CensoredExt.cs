using AutoInority.Extentions;

namespace AutoInority.Creature
{
    internal class CensoredExt : BaseCreatureExt
    {
        protected override SkillTypeInfo[] DefaultSkills { get; } = new SkillTypeInfo[] { Instinct, Insight, Attachment };

        public CensoredExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (agent.level < 5) // Never send agent less than level V though he may survive.
            {
                message = string.Format(Angela.Creature.Censored, agent.Tag(), _creature.Tag(), skill.Tag());
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }

        public override bool CheckConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            var normalConfidence = NormalConfidence(agent, skill);
            if (normalConfidence < Automaton.Instance.CounterDecreaseConfidence)
            {
                return false;
            }
            else if (_creature.qliphothCounter == 1)
            {
                return normalConfidence > Automaton.Instance.CreatureEscapeConfidence;
            }
            return base.CheckConfidence(agent, skill);
        }
    }
}