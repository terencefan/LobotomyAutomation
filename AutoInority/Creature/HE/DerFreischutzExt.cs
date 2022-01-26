namespace AutoInority.Creature
{
    internal class DerFreischutzExt : ExpectGoodAndNormalExt
    {
        protected override SkillTypeInfo[] NormalSkillSets { get; } = { Instinct, Insight, Repression };

        protected override SkillTypeInfo[] GoodSkillSets { get; } = { Repression };

        public DerFreischutzExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (agent.justiceLevel <= 3)
            {
                message = Message(Angela.Creature.DerFreischutzExt, agent, skill);
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }
    }
}