using AutoInority.Extentions;

namespace AutoInority.Creature
{
    internal class HappyTeddyExt : BaseCreatureExt
    {
        protected override SkillTypeInfo[] DefaultSkills { get; } = new SkillTypeInfo[] { Insight, Attachment, Repression };

        public HappyTeddyExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            var record = CenterBrain.FindLastRecord(_creature);
            if (record != null && record.Agent.name == agent.name)
            {
                message = string.Format(Angela.Creature.HappyTeddy, agent.Tag(), _creature.Tag(), skill.Tag());
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }
    }
}