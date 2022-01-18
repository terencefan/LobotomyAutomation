namespace AutoInority.Creature
{
    internal class NakedNestExt : BaseCreatureExt
    {
        protected override SkillTypeInfo[] DefaultSkills { get; } = new SkillTypeInfo[] { Instinct, Attachment, Repression };

        public NakedNestExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CheckConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            return GoodConfidence(agent, skill) > Automaton.Instance.DeadConfidence && base.CheckConfidence(agent, skill);
        }
    }
}