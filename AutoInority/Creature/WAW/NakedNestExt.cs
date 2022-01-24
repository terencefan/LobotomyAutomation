namespace AutoInority.Creature
{
    internal class NakedNestExt : BaseCreatureExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Instinct, Attachment, Repression };

        public NakedNestExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CheckConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            if (IsOverloaded)
            {
                return NormalConfidence(agent, skill) > Automaton.Instance.DeadConfidence && base.CheckConfidence(agent, skill);
            }
            else
            {
                return GoodConfidence(agent, skill) > Automaton.Instance.DeadConfidence && base.CheckConfidence(agent, skill);
            }
        }
    }
}