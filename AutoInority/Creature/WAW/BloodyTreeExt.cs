namespace AutoInority.Creature
{
    internal class BloodyTreeExt : BaseCreatureExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Insight, Attachment, Repression };

        public BloodyTreeExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CheckConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            if (QliphothCounter == 1)
            {
                return NormalConfidence(agent, skill) > Automaton.Instance.CreatureEscapeConfidence && base.CheckConfidence(agent, skill);
            }
            return base.CheckConfidence(agent, skill);
        }
    }
}