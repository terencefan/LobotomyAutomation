namespace AutoInority.Creature
{
    internal class NakedNestExt : BaseCreatureExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Instinct, Attachment, Repression };

        public NakedNestExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (IsOverloaded && NormalConfidence(agent, skill) < DeadConfidence)
            {
                message = "";
                return false;
            }
            else if (GoodConfidence(agent, skill) < DeadConfidence)
            {
                message = "";
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }
    }
}