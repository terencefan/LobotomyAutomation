using AutoInority.Extentions;

namespace AutoInority.Creature
{
    internal class RedShoesExt : BaseCreatureExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Instinct, Insight, Attachment };

        public RedShoesExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (agent.temperanceLevel < 3)
            {
                message = string.Format(Angela.Creature.RedShoes, agent.Tag(), _creature.Tag(), skill.Tag());
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }

        public override bool CheckConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            return GoodConfidence(agent, skill) > Automaton.Instance.CreatureEscapeConfidence && base.CheckConfidence(agent, skill);
        }
    }
}