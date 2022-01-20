namespace AutoInority.Creature
{
    internal class ExpectNormalExt : BaseCreatureExt
    {
        public ExpectNormalExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CheckConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            return NormalConfidence(agent, skill) > Automaton.Instance.CreatureEscapeConfidence && base.CheckConfidence(agent, skill);
        }
    }
}
