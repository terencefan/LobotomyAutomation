namespace AutoInority.Creature
{
    internal class GalaxyBoyExt : BaseCreatureExt
    {
        public override bool IsUrgent => QliphothCounter < 5;

        public GalaxyBoyExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CheckConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            return (agent.HasUnitBuf(UnitBufType.FRIEND_TOKEN) || CenterBrain.BlessedCount < 3) && base.CheckConfidence(agent, skill);
        }
    }
}