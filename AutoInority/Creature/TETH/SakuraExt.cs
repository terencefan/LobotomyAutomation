using AutoInority.Extentions;

namespace AutoInority.Creature
{
    internal class SakuraExt : BaseCreatureExt
    {
        public SakuraExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            var rate = LessThanGoodConfidence(agent, skill);
            if (QliphothCounter == 1 && rate < 0.8f)
            {
                message = Message(Angela.Creature.Sakura, agent, skill);
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }

        public override bool CheckConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            var rate = LessThanGoodConfidence(agent, skill);
            if (QliphothCounter > 1 && rate < Automaton.Instance.CounterDecreaseConfidence)
            {
                return false;
            }
            else if (QliphothCounter == 1 && rate < Automaton.Instance.DeadConfidence)
            {
                return false;
            }
            return base.CheckConfidence(agent, skill);
        }

        private double LessThanGoodConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            var workProb = CalculateWorkSuccessProb(agent, skill);
            return Confidence.InRange(_creature.MaxCube(), workProb, 0, _creature.GoodBound() - 1);
        }
    }
}