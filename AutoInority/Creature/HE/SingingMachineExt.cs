using AutoInority.Extentions;

namespace AutoInority.Creature
{
    internal class SingingMachineExt : BaseCreatureExt
    {
        protected override SkillTypeInfo[] DefaultSkills { get; } = new SkillTypeInfo[] { Instinct, Insight, Repression };

        public SingingMachineExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (agent.Rstat > 3 || agent.Bstat < 3)
            {
                message = string.Format(Angela.Creature.SiningMachine, agent.Tag(), _creature.Tag(), skill.Tag());
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }
    }
}