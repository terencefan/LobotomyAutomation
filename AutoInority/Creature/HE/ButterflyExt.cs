namespace AutoInority.Creature
{
    internal class ButterflyExt : BaseCreatureExt
    {
        public override bool AutoSuppress => true;

        protected override SkillTypeInfo[] DefaultSkills { get; } = new SkillTypeInfo[] { Insight, Repression };

        private bool IsFarming => Automaton.Instance.FarmingCreatures.Contains(_creature);

        public ButterflyExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (IsFarming)
            {
                return base.CanWorkWith(agent, skill, out message);
            }
            else if (agent.Rstat > 3 || agent.Pstat < 3)
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
    }
}