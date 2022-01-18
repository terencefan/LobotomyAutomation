namespace AutoInority.Creature
{
    class SnowQueenExt : GoodNormalExt
    {
        public SnowQueenExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (agent.Equipment.HasEquipment(1021) && GoodConfidence(agent, skill) < DeadConfidence)
            {
                message = Message(Angela.Creatures.SnowQueen, agent, skill);
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }

        public override bool IsUrgent()
        {
            return base.IsUrgent();
        }

        public override SkillTypeInfo[] SkillSets()
        {
            return new SkillTypeInfo[] { Instinct, Insight, Attachment };
        }
    }
}
