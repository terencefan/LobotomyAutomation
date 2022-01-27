namespace AutoInority.Creature
{
    internal class ExpectGoodAndNormalExt : BaseCreatureExt
    {
        public override sealed SkillTypeInfo[] SkillSets => QliphothCounter > 1 ? NormalSkillSets : GoodSkillSets;

        protected virtual SkillTypeInfo[] GoodSkillSets => NormalSkillSets;

        protected virtual SkillTypeInfo[] NormalSkillSets { get; } = All;

        public ExpectGoodAndNormalExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CheckConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            return CheckGoodNormal(agent, skill) && base.CheckConfidence(agent, skill);
        }

        protected virtual bool CheckGoodNormal(AgentModel agent, SkillTypeInfo skill)
        {
            if (QliphothCounter > 1 || (IsFarming && AutoSuppress))
            {
                return NormalConfidence(agent, skill) > Automaton.Instance.CounterDecreaseConfidence;
            }
            return IsOverloaded || GoodConfidence(agent, skill) > CreatureEscapeConfidence;
        }
    }
}