namespace AutoInority.Creature
{
    internal class PpodaeExt : ExpectNormalExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Instinct };

        public PpodaeExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (skill.rwbpType != RwbpType.R)
            {
                message = Message(Angela.Creature.Ppodae, agent, skill);
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }
    }
}