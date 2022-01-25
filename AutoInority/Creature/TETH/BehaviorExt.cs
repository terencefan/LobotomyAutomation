using System.Reflection;

namespace AutoInority.Creature
{
    internal class BehaviorExt : EquipKitExt
    {
        private readonly FieldInfo _equipElapsedTime = typeof(JusticeReceiver.JusticeReceiverKit).GetField(nameof(_equipElapsedTime),
                                                                                                           BindingFlags.NonPublic | BindingFlags.Instance);

        public BehaviorExt(CreatureModel kit) : base(kit)
        {
        }

        public override bool CanReturn(AgentModel agent)
        {
            return (float)_equipElapsedTime.GetValue(Script.kitEvent) > 30;
        }
    }
}