namespace AutoInority.Creature
{
    internal class WhiteNightExt : BaseCreatureExt
    {
        public override bool IsUrgent => QliphothCounter < 3;

        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Insight, Attachment, Repression };

        public WhiteNightExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CheckWorkConfidence(AgentModel agent, SkillTypeInfo skill) => NormalConfidence(agent, skill) > DeadConfidence;
    }
}