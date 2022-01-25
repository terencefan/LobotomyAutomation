using System.Reflection;

namespace AutoInority.Creature
{
    internal class MeatIdolExt : ChannelKitExt
    {
        private readonly FieldInfo _damageTimer = typeof(MeatIdol).GetField(nameof(_damageTimer), BindingFlags.NonPublic | BindingFlags.Instance);

        public MeatIdolExt(CreatureModel kit) : base(kit)
        {
        }

        public override bool CanStop()
        {
            return ((Timer)_damageTimer.GetValue(Script)).elapsed > 20;
        }

        public override bool CanUse(AgentModel agent)
        {
            return agent.hp > 60;
        }
    }
}