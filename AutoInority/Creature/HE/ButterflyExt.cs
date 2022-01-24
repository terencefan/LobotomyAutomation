namespace AutoInority.Creature
{
    internal class ButterflyExt : BaseCreatureExt
    {
        public override bool AutoSuppress => true;

        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Insight, Repression };

        public ButterflyExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (agent.justiceLevel < 3)
            {
                message = Message(Angela.Creature.Butterfly, agent, skill);
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }

        public override bool CheckConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            return NormalConfidence(agent, skill) > CreatureEscapeConfidence && base.CheckConfidence(agent, skill);
        }

        public override float ConfidencePenalty(AgentModel agent, SkillTypeInfo skill) => agent.fortitudeLevel > 3 ? 0.2f : 0;
    }
}