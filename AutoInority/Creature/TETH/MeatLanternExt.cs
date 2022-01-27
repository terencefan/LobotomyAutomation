namespace AutoInority.Creature
{
    internal class MeatLanternExt : ExpectNormalExt
    {
        public MeatLanternExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (CalculateWorkTime(agent) < 41)
            {
                message = "";
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }
    }
}