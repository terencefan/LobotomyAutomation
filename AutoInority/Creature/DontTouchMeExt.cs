namespace AutoInority.Creature
{
    internal class DontTouchMeExt : BaseCreatureExt
    {
        public DontTouchMeExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            message = null;
            return false;
        }
    }
}