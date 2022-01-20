using System.Reflection;

namespace AutoInority.Creature
{
    internal class BaldExt : BaseCreatureExt
    {
        private readonly MethodInfo _isBald;

        public BaldExt(CreatureModel creature) : base(creature)
        {
            _isBald = typeof(Bald).GetMethod("IsBald");
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (_creature.isOverloaded)
            {
                return base.CanWorkWith(agent, skill, out message);
            }
            return base.CanWorkWith(agent, skill, out message) && (bool)_isBald.Invoke(_creature.script, new object[] { agent });
        }
    }
}