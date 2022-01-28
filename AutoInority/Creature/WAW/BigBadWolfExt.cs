using System.Collections.Generic;
using System.Reflection;

namespace AutoInority.Creature
{
    internal class BigBadWolfExt : ExpectNormalExt
    {
        private static readonly FieldInfo eatenWorker = typeof(BigBadWolf).GetField(nameof(eatenWorker), BindingFlags.Instance | BindingFlags.NonPublic);

        public override bool IsUrgent => Eatens.Count > 0;

        public override SkillTypeInfo[] SkillSets { get; } = { Instinct, Attachment };

        private List<Uncontrollable_WolfEaten> Eatens => (List<Uncontrollable_WolfEaten>)eatenWorker.GetValue(Script);

        public BigBadWolfExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (agent.GetRecentWorkedCreature()?.script is RedHood)
            {
                message = Message(Angela.Creature.BigBadWolf, agent, skill);
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }

        public override bool CheckWorkConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            if (IsUrgent)
            {
                return skill.rwbpType == RwbpType.R && agent.fortitudeLevel >= 4;
            }
            return base.CheckWorkConfidence(agent, skill);
        }
    }
}
