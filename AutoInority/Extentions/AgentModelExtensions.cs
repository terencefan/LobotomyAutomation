using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoInority.Extentions
{
    public static class AgentModelExtensions
    {
        public static IEnumerable<AgentModel> FilterCanSuppress(this IEnumerable<AgentModel> agents, CreatureModel creature)
        {
            return agents.Where(x => x.IsAvailable() && x.IsCapableOfPressing(creature)).ToList();
        }

        public static Sefira GetActualSefira(this AgentModel agent)
        {
            MovableObjectNode node = null;
            PassageObjectModel passage = null;
            Sefira sefira = null;
            try
            {
                node = agent.GetMovableNode();
                passage = node.GetPassage();
                sefira = passage.GetSefira();
                return sefira;
            }
            catch (Exception e)
            {
                Log.Error(e);
                Log.Info($"agent: {agent.name}, node: {node == null}, passage: {passage == null}, sefira: {sefira == null}");
                return agent.GetCurrentSefira();
            }
        }

        public static bool HasAnotherGift(this AgentModel agent, EquipmentTypeInfo gift)
        {
            var regionId = UnitEGOgiftSpace.GetRegionId(gift);
            return agent.GetAllGifts().Where(x => UnitEGOgiftSpace.GetRegionId(x.metaInfo) == regionId).Any();
        }

        public static bool HasGift(this AgentModel agent, CreatureModel creature, out EquipmentTypeInfo gift)
        {
            return creature.GetExtension().TryGetEGOGift(out gift) && agent.HasGift(gift);
        }

        public static bool HasGift(this AgentModel agent, EquipmentTypeInfo gift) => agent.Equipment.gifts.HasEquipment(gift.id);

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

        public static bool IsRegionLocked(this AgentModel agent, EquipmentTypeInfo gift, out string slotName)
        {
            slotName = UnitEGOgiftSpace.GetRegionName(UnitEGOgiftSpace.GetRegionId(gift));
            return agent.Equipment.gifts.GetLockState(gift);
        }

        public static void RemoveAutomatonBuff(this AgentModel agent)
        {
            var buff = agent.GetUnitBufList().Where(buf => buf is AutomatonBuf);
            if (buff.Any())
            {
                agent.RemoveUnitBuf(buff.First());
            }
        }

        public static string Tag(this AgentModel agent) => $"<color=#66bfcd>{agent.name}</color>";

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