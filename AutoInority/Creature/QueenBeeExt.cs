namespace AutoInority.Creature
{
    internal class QueenBeeExt : BaseCreatureExt
    {
        protected override SkillTypeInfo[] DefaultSkills { get; } = new SkillTypeInfo[] { Instinct, Insight, Attachment };

        public QueenBeeExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CheckConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            return CheckGoodNormal(agent, skill) && base.CheckConfidence(agent, skill);
        }
    }
}