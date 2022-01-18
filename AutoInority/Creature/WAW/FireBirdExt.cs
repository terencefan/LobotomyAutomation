using AutoInority.Extentions;

namespace AutoInority.Creature
{
    internal class FireBirdExt : BaseCreatureExt
    {
        public FireBirdExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            var workProb = CalculateWorkSuccessProb(agent, skill);
            var confidence = Confidence.InRange(_creature.MaxCube(), workProb, 0, _creature.NormalBound() - 1);

            if (_creature.qliphothCounter == 1 && confidence < 0.8)
            {
                message = string.Format(Angela.Creature.FireBird, agent.Tag(), _creature.Tag(), skill.Tag());
                return false;
            }
            else if (_creature.qliphothCounter == 2 && confidence < 0.5)
            {
                message = string.Format(Angela.Creature.FireBirdEdge, agent.Tag(), _creature.Tag(), skill.Tag());
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }

        public override bool CheckConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            var normalBound = _creature.NormalBound();
            var confidence = Confidence.InRange(_creature.MaxCube(), CalculateWorkSuccessProb(agent, skill), 0, normalBound - 1);
            return confidence < Automaton.Instance.CreatureEscapeConfidence && base.CheckConfidence(agent, skill);
        }
    }
}