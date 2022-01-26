namespace AutoInority.Creature
{
    internal class SpiderBudExt : BaseCreatureExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Instinct, Attachment, Repression };

        public SpiderBudExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (agent.prudenceLevel < 2 || skill.rwbpType == RwbpType.W)
            {
                message = Message(Angela.Creature.Spider, agent, skill);
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }
    }
}