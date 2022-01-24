using System.Reflection;

namespace AutoInority.Creature
{
    class YouMustBeHappyExt : ChannelKitExt
    {
        private readonly FieldInfo _succeedUsing = typeof(YouMustHappy).GetField(nameof(_succeedUsing), BindingFlags.NonPublic | BindingFlags.Instance);

        public YouMustBeHappyExt(CreatureModel kit) : base(kit)
        {
        }

        public override bool CanStop() => (bool)_succeedUsing.GetValue(_kit.script);
    }
}
