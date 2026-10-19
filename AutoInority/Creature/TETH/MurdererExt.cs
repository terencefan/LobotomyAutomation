namespace AutoInority.Creature
{
    internal class MurdererExt : BaseCreatureExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Instinct, Attachment };

        public MurdererExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CheckWorkConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            return IsOverloaded || IsExploiting || NormalConfidence(agent, skill) > CreatureEscapeConfidence;
        }
    }
}