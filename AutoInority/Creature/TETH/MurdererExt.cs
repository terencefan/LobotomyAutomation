namespace AutoInority.Creature
{
    internal class MurdererExt : BaseCreatureExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Instinct, Attachment };

        public MurdererExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CheckConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            return NormalConfidence(agent, skill) > Automaton.Instance.CreatureEscapeConfidence && base.CheckConfidence(agent, skill);
        }
    }
}