using AutoInority.Extentions;

namespace AutoInority.Creature
{
    internal class FireBirdExt : BaseCreatureExt
    {
        public FireBirdExt(CreatureModel creature) : base(creature)
        {
        }

        public override float GetConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            var workProb = CalculateWorkSuccessProb(agent, skill);
            return Confidence.InRange(_creature.MaxCube(), workProb, 0, _creature.NormalBound());
        }
    }
}