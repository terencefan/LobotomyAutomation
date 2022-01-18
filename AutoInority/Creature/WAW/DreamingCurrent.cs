namespace AutoInority.Creature
{
    internal class DreamingCurrent : BaseCreatureExt
    {
        protected override SkillTypeInfo[] DefaultSkills => new SkillTypeInfo[] { Instinct, Attachment, Repression };

        public DreamingCurrent(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (agent.temperanceLevel == 1)
            {
                message = Message(Angela.Creature.Shark, agent, skill);
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }
    }
}
