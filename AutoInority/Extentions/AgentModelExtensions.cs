using System.Collections.Generic;
using System.Linq;

namespace AutoInority.Extentions
{
    public static class AgentModelExtensions
    {
        public static bool EGOSlotLocked(this AgentModel agent, CreatureEquipmentMakeInfo gift) => agent.Equipment.gifts.GetLockState(gift.equipTypeInfo);

        public static IEnumerable<AgentModel> FilterSuppress(this IEnumerable<AgentModel> agents, CreatureModel creature)
        {
            return agents.Where(x => x.IsAvailable() && x.IsCapableOfPressing(creature)).ToList();
        }

        public static bool HasEGOGift(this AgentModel agent, CreatureModel creature, out CreatureEquipmentMakeInfo gift)
        {
            if (creature.TryGetEGOGift(out gift))
            {
                return agent.Equipment.gifts.HasEquipment(gift.equipTypeInfo.id);
            }
            return true;
        }

        public static bool HasReachedExpLimit(this AgentModel agent, RwbpType type, out string name)
        {
            name = null;
            if (type == RwbpType.R)
            {
                name = LocalizeTextDataModel.instance.GetText("Rstat");
                return agent.primaryStat.maxHP + agent.primaryStatExp.hp >= WorkerPrimaryStat.MaxStatR();
            }
            else if (type == RwbpType.W)
            {
                name = LocalizeTextDataModel.instance.GetText("Wstat");
                return agent.primaryStat.maxMental + agent.primaryStatExp.mental >= WorkerPrimaryStat.MaxStatW();
            }
            else if (type == RwbpType.B)
            {
                name = LocalizeTextDataModel.instance.GetText("Bstat");
                return agent.primaryStat.workProb + agent.primaryStatExp.work >= WorkerPrimaryStat.MaxStatB();
            }
            else if (type == RwbpType.P)
            {
                name = LocalizeTextDataModel.instance.GetText("Pstat");
                return agent.primaryStat.attackSpeed + agent.primaryStatExp.battle >= WorkerPrimaryStat.MaxStatP();
            }
            return false;
        }

        public static bool IsAvailable(this AgentModel agent)
        {
            return agent.hp == agent.maxHp && agent.mental == agent.maxMental && agent.GetState() == AgentAIState.IDLE;
        }

        public static string Tag(this AgentModel agent) => $"<color=#66bfcd>{agent.name}</color>";

        /// <summary>
        /// Lv 4 agent can suppress WAW
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="creature"></param>
        /// <returns></returns>
        private static bool IsCapableOfPressing(this AgentModel agent, CreatureModel creature)
        {
            var riskLevel = creature.GetRiskLevel();
            var weapon = agent.Equipment.weapon.metaInfo;
            var weaponGrade = (int)weapon.Grade;
            var armor = agent.Equipment.weapon.metaInfo;
            var armorGrade = (int)armor.Grade;

            if (agent.level > riskLevel)
            {
                return true;
            }
            else if (agent.level < riskLevel)
            {
                return false;
            }

            var defense = creature.metaInfo.defenseTable.GetDefenseInfo();
            if (weaponGrade > riskLevel)
            {
                return armorGrade >= riskLevel;
            }
            else if (defense.GetMultiplier(weapon.damageInfo.type) > 1.0f)
            {
                return armorGrade > riskLevel;
            }
            return false;
        }
    }
}