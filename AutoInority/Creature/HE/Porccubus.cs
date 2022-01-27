namespace AutoInority.Creature.TETH
{
    internal class PorccubusExt : ExpectGoodAndNormalExt
    {
        protected override SkillTypeInfo[] NormalSkillSets { get; } = { Instinct, Insight, Attachment };

        protected override SkillTypeInfo[] GoodSkillSets { get; } = { Instinct };

        public PorccubusExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (agent.temperanceLevel < 4 && GoodConfidence(agent, skill) > 1 - DeadConfidence)
            {
                message = Message(Angela.Creature.Porccubus, agent, skill);
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }
    }
}