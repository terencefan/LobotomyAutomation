namespace AutoInority.Creature
{
    internal class AlriuneExt : BaseCreatureExt
    {
        public AlriuneExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CheckWorkConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            return NormalConfidence(agent, skill) > 0.8f;
        }
    }
}
