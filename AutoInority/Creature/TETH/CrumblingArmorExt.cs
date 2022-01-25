using AutoInority.Extentions;

namespace AutoInority.Creature
{
    internal class CrumblingArmorExt : BaseCreatureExt
    {
        public static readonly int GIFT = 4000371;

        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Instinct, Repression };

        public CrumblingArmorExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (agent.fortitudeLevel == 1)
            {
                message = Message(Angela.Creature.ArmorKill, agent, skill);
                return false;
            }
            else if (skill.rwbpType == RwbpType.P)
            {
                if (IsFarming && agent.HasReachedExpLimit(RwbpType.B, out _) && agent.HasReachedExpLimit(RwbpType.P, out _) && !agent.HasEquipment(GIFT))
                {
                    return base.CanWorkWith(agent, skill, out message);
                }
                message = Message(Angela.Creature.ArmorWarning, agent, skill);
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }

        public override bool TryGetEGOGift(out EquipmentTypeInfo gift)
        {
            gift = EquipmentTypeList.instance.GetData(GIFT);
            return gift != null;
        }
    }
}