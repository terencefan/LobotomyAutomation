namespace AutoInority.Creature
{
    internal class SakuraExt : BaseCreatureExt
    {
        public SakuraExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (IsOverloaded || IsFarming)
            {
                return base.CanWorkWith(agent, skill, out message);
            }
            else if (LessThanGoodConfidence(agent, skill) < 0.8f && QliphothCounter == 1)
            {
                message = Message(Angela.Creature.Sakura, agent, skill);
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }
    }
}