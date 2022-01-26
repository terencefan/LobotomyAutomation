namespace AutoInority.Creature.TETH
{
    internal class PorccubusExt : ExpectGoodAndNormalExt
    {
        private static SkillTypeInfo[] TypeNormal { get; } = { Instinct, Insight, Attachment };

        private static SkillTypeInfo[] TypeGood { get; } = { Instinct };

        public override SkillTypeInfo[] SkillSets => QliphothCounter == 1 ? TypeGood : TypeNormal;

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
