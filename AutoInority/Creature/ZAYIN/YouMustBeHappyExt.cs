using System.Collections;
using System.Reflection;

namespace AutoInority.Creature
{
    class YouMustBeHappyExt : ChannelKitExt
    {
        private const int MaxAutoUses = 4;

        private readonly FieldInfo _succeedUsing = typeof(YouMustHappy).GetField(nameof(_succeedUsing), BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly FieldInfo _usedAgentList = typeof(YouMustHappy).GetField("_usedAgentList", BindingFlags.NonPublic | BindingFlags.Instance);

        public YouMustBeHappyExt(CreatureModel kit) : base(kit)
        {
        }

        public override bool CanStop() => (bool)_succeedUsing.GetValue(_kit.script);

        public override bool CanUse(AgentModel agent)
        {
            // 同一员工第 6 次使用即失控死亡（OnReleaseWork usedCount>=6 → LoseControl+Dead）；
            // 自动化最多用 4 次，第 5 次留给主管手动，留一次余量
            var list = _usedAgentList.GetValue(Script) as IEnumerable;
            if (list == null)
            {
                return true;
            }
            foreach (var entry in list)
            {
                var usedAgent = entry.GetType().GetField("usedAgent").GetValue(entry) as AgentModel;
                if (usedAgent != agent)
                {
                    continue;
                }
                return (int)entry.GetType().GetField("usedCount").GetValue(entry) < MaxAutoUses;
            }
            return true;
        }
    }
}
