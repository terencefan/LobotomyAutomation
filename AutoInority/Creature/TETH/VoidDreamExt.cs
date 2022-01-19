namespace AutoInority.Creature
{
    internal class VoidDreamExt : GoodNormalExt
    {
        public VoidDreamExt(CreatureModel creature) : base(creature)
        {
        }

        protected override SkillTypeInfo[] DefaultSkills => new SkillTypeInfo[] { Instinct, Insight, Attachment };

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (agent.temperanceLevel < 2)
            {
                message = Message(Angela.Creature.VoidDream, agent, skill);
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }
    }
}