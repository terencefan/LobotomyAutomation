namespace AutoInority.Creature
{
    internal class BigBirdExt : BaseCreatureExt
    {
        protected override SkillTypeInfo[] DefaultSkills { get; } = new SkillTypeInfo[] { Instinct, Attachment };

        public BigBirdExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (NormalConfidence(agent, skill) < Automaton.Instance.CreatureEscapeConfidence && _creature.qliphothCounter < 3)
            {
                message = Message(Angela.Creature.BigBird, agent, skill);
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }

        public override bool IsUrgent()
        {
            return _creature.qliphothCounter < 5;
        }
    }
}