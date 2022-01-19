using AutoInority.Extentions;

namespace AutoInority.Creature
{
    internal class NothingExt : BaseCreatureExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Instinct, Attachment };

        public NothingExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (agent.justiceLevel < 4)
            {
                message = string.Format(Angela.Creature.Nothing, agent.Tag(), _creature.Tag(), skill.Tag());
                return false;
            }

            var workSuccessProb = CalculateWorkSuccessProb(agent, skill);
            var confidence = Confidence.InRange(_creature.MaxCube(), workSuccessProb, _creature.NormalBound());

            if (confidence < Automaton.Instance.DeadConfidence)
            {
                message = string.Format(Angela.Creature.Nothing, agent.Tag(), _creature.Tag(), skill.Tag());
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }
    }
}