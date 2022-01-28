using System.Reflection;

using AutoInority.Extentions;

namespace AutoInority.Creature
{
    internal class HellTrainExt : BaseKitExt
    {
        private FieldInfo _otherCreatureWorkCount = typeof(HellTrain).GetField(nameof(_otherCreatureWorkCount), BindingFlags.NonPublic | BindingFlags.Instance);

        public HellTrainExt(CreatureModel kit) : base(kit)
        {
        }

        public override bool CanUse(AgentModel agent) => true;

        public override void OnFixedUpdate()
        {
            if ((int)_otherCreatureWorkCount.GetValue(Script) < 3)
            {
                return;
            }
            else if (_kit.IsAvailable())
            {
                Handle();
            }
            base.OnFixedUpdate();
        }
    }
}