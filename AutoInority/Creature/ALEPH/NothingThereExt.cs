using AutoInority.Extentions;

namespace AutoInority.Creature
{
    internal class NothingThereExt : BaseCreatureExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Instinct, Attachment };

        public NothingThereExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (agent.justiceLevel < 4)
            {
                message = string.Format(Angela.Creature.Nothing, agent.Tag(), _creature.Tag(), skill.Tag());
                return false;
            }
            else if (NormalConfidence(agent, skill) < DeadConfidence)
            {
                message = string.Format(Angela.Creature.Nothing, agent.Tag(), _creature.Tag(), skill.Tag());
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }
    }
}