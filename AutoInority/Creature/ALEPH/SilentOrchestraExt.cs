using AutoInority.Extentions;

namespace AutoInority.Creature
{
    internal class SilentOrchestraExt : BaseCreatureExt
    {
        private readonly int _max;

        private readonly int _from;

        private readonly int _to;

        public SilentOrchestraExt(CreatureModel creature) : base(creature)
        {
            _max = _creature.MaxCube();
            _from = _creature.NormalBound() + 1;
            _to = _creature.GoodBound();
        }

        public override bool CheckWorkConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            var rate = CalculateWorkSuccessProb(agent, skill);
            return Confidence.InRange(_max, rate, _from, _to) > 0.65f;
        }
    }
}
