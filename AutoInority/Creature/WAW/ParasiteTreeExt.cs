using System.Collections.Generic;
using System.Reflection;

namespace AutoInority.Creature
{
    internal class ParasiteTreeExt : BaseCreatureExt
    {
        private readonly FieldInfo blessedList = typeof(Yggdrasil).GetField(nameof(blessedList), BindingFlags.NonPublic | BindingFlags.Instance);

        public override SkillTypeInfo[] SkillSets { get; } = { Instinct, Insight, Attachment };

        private List<WorkerModel> BlessedList => (List<WorkerModel>)blessedList.GetValue(Script);

        public ParasiteTreeExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (BlessedList.Count >= 4 && !BlessedList.Contains(agent))
            {
                message = null;
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }
    }
}