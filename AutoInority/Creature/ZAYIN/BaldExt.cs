using System.Reflection;

namespace AutoInority.Creature
{
    internal class BaldExt : BaseCreatureExt
    {
        private readonly MethodInfo _isBald = typeof(Bald).GetMethod("IsBald", BindingFlags.NonPublic | BindingFlags.Instance);

        public BaldExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (IsOverloaded)
            {
                return base.CanWorkWith(agent, skill, out message);
            }
            return base.CanWorkWith(agent, skill, out message) && (bool)_isBald.Invoke(Script, new object[] { agent });
        }
    }
}