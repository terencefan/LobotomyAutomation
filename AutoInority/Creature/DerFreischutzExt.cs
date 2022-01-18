namespace AutoInority.Creature
{
    class DerFreischutzExt : GoodNormalExt
    {
        protected override SkillTypeInfo[] DefaultSkills => new SkillTypeInfo[] { Repression };

        public DerFreischutzExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (agent.justiceLevel <= 3)
            {
                message = Message(Angela.Creatures.DerFreischutzExt, agent, skill);
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }
    }
}
