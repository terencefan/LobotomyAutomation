using AutoInority.Extentions;

namespace AutoInority.Creature
{
    internal class CensoredExt : ExpectNormalExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Instinct, Insight, Attachment };

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

        protected override float MentalFix(float mental) => 0.4f * mental;
    }
}