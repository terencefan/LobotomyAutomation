namespace AutoInority.Creature
{
    internal class MeltingLoveExt : BaseCreatureExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = { Repression };

        public MeltingLoveExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (skill.rwbpType != RwbpType.P)
            {
                message = Message(Angela.Creature.MeltingLove, agent, skill);
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }

        public override bool CheckWorkConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            return IsOverloaded || IsFarming;
        }
    }
}
