namespace AutoInority.Creature
{
    internal class BlackSwanExt : BaseCreatureExt
    {
        protected override SkillTypeInfo[] DefaultSkills { get; } = new SkillTypeInfo[] { Instinct, Insight, Repression };

        public BlackSwanExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CheckConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            if (_creature.qliphothCounter == 2 && GoodConfidence(agent, skill) < Automaton.Instance.CounterDecreaseConfidence)
            {
                return false;
            }
            else if (_creature.qliphothCounter == 1 && NormalConfidence(agent, skill) < Automaton.Instance.CreatureEscapeConfidence)
            {
                return false;
            }
            return base.CheckConfidence(agent, skill);
        }
    }
}