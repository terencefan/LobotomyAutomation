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

            if (IsOverloaded || IsFarming)
            {
                return base.CanWorkWith(agent, skill, out message);
            }
            else if (QliphothCounter > 1 && confidence < 0.5)
            {
                message = string.Format(Angela.Creature.FireBirdEdge, agent.Tag(), _creature.Tag(), skill.Tag());
                return false;
            }
            else if (QliphothCounter == 1 && confidence < 0.8)
            {
                message = string.Format(Angela.Creature.FireBird, agent.Tag(), _creature.Tag(), skill.Tag());
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }
    }
}