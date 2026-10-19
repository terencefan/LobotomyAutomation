using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoInority.Extentions
{
    public static class AgentModelExtensions
    {
        private static readonly RwbpType[] AllTypes = new RwbpType[] { RwbpType.P, RwbpType.R, RwbpType.W, RwbpType.B };

        // 护盾剩余量的读取方式：BarrierBuf 的 _barrierValue（剩余抵扣量）与 _rwbpType 均为私有字段且无公开
        // 读取 API；UnitModel._barrierBufList 是 protected（GetUnitBufList() 返回的是基类 UnitBuf 列表）。
        // 游戏唯一的消耗入口 UseBarrier 会真实抵扣并在耗尽时 Destroy() buf，有副作用；而评估（CheckSurvive）
        // 会被重复触发，必须无副作用，因此只能反射读取字段值，绝不调用 UseBarrier。
        private static readonly FieldInfo BarrierBufTypeField = typeof(BarrierBuf).GetField("_rwbpType", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo BarrierBufValueField = typeof(BarrierBuf).GetField("_barrierValue", BindingFlags.NonPublic | BindingFlags.Instance);

        // 对应伤害类型的护盾当前剩余抵扣量（只读评估，无副作用）。
        // RwbpType.A 的通用护盾（UseBarrier 对任意类型抵扣）一并计入；耗尽的护盾 _barrierValue 已归零，读 0 即可。
        public static float GetBarrierValue(this AgentModel agent, RwbpType type)
        {
            if (BarrierBufTypeField == null || BarrierBufValueField == null)
            {
                return 0;
            }
            var value = 0f;
            foreach (var buf in agent.GetUnitBufList())
            {
                if (!(buf is BarrierBuf barrier))
                {
                    continue;
                }
                var bufType = (RwbpType)BarrierBufTypeField.GetValue(barrier);
                if (bufType == type || bufType == RwbpType.A)
                {
                    value += (float)BarrierBufValueField.GetValue(barrier);
                }
            }
            return value;
        }

        public static IEnumerable<AgentModel> FilterExploit(this IEnumerable<AgentModel> agents, CreatureModel creature)
        {
            if (creature.GetExtension().TryGetEGOGift(out var gift))
            {
                return agents.Where(x => !x.HasGift(gift) && !x.IsRegionLocked(gift) && !x.HasBetterGift(gift));
            }
            return agents;
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
                Log.Debug($"agent: {agent.name}, node: {node == null}, passage: {passage == null}, sefira: {sefira == null}");
                return agent.GetCurrentSefira();
            }
        }

        public static bool HasAnotherGift(this AgentModel agent, EquipmentTypeInfo gift)
        {
            if (gift == null)
            {
                return false;
            }
            var regionId = UnitEGOgiftSpace.GetRegionId(gift);
            return agent.GetAllGifts().Where(x => UnitEGOgiftSpace.GetRegionId(x.metaInfo) == regionId).Any();
        }

        public static bool HasBetterGift(this AgentModel agent, EquipmentTypeInfo target)
        {
            var regionId = UnitEGOgiftSpace.GetRegionId(target);
            var priority = target.GetPriority();
            foreach (var gift in agent.GetAllGifts().Where(x => UnitEGOgiftSpace.GetRegionId(x.metaInfo) == regionId))
            {
                if (gift.metaInfo.GetPriority() > priority)
                {
                    // Log.Info($"{agent.name} has better gift: {gift.metaInfo.Name}");
                    return true;
                }
            }
            return false;
        }

        public static bool HasGift(this AgentModel agent, CreatureModel creature, out EquipmentTypeInfo gift)
        {
            return creature.GetExtension().TryGetEGOGift(out gift) && agent.HasGift(gift);
        }

        public static bool HasGift(this AgentModel agent, EquipmentTypeInfo gift) => agent.Equipment.gifts.HasEquipment(gift.id);

        /// <summary>
        /// 聚合判定：员工是否还有可练的经验维度（types = 未满级类型集合）。
        /// 注意与单类型版 HasReachedExpLimit(type) 的布尔语义相反——原聚合版
        /// 误用 HasReachedExpLimit 命名且调用方守卫随之反转，训练从不派工
        /// （历史 bug，2026-10 修正并改名）。
        /// </summary>
        public static bool HasTrainableTypes(this AgentModel agent, out HashSet<RwbpType> types)
        {
            types = new HashSet<RwbpType>();
            foreach (var type in AllTypes)
            {
                if (!agent.HasReachedExpLimit(type, out _))
                {
                    types.Add(type);
                }
            }
            return types.Count > 0;
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
            // 满血满精神的原要求与 CheckSurvive 的数学门槛双重保守——置信度模型
            // 本来就在用当前 hp/mental 计算「带伤能否扛住这次工作」。放宽为 90%：
            // 轻伤职工可进候选池，生死仍由 CheckSurvive 按当前数值把关
            // 白夜的「卖主的叛徒」无敌且只能赎罪，不能作为普通劳动力进候选池
            return agent.hp >= agent.maxHp * 0.9f && agent.mental >= agent.maxMental * 0.9f && agent.GetState() == AgentAIState.IDLE
                && !agent.HasUnitBuf(UnitBufType.DEATH_ANGEL_BETRAYER);
        }

        public static bool IsCapableOfPressing(this AgentModel agent, CreatureModel creature)
        {
            var riskLevel = creature.GetRiskLevel();
            var weapon = agent.Equipment.weapon.metaInfo;
            var weaponGrade = (int)weapon.Grade;
            // 武器伤害效能：异想体当前防御行对武器伤害类型乘数低于全局阈值
            // （AutomationConfig.PressingEfficacyThreshold，默认 0.5 = RESISTANCE/IMMUNE）视为打不动——
            // 属性等级再高也不派去镇压（R/W/B/P 之外的伤害类型恒 1.0 天然放行）
            if (creature.defense.GetMultiplier(weapon.damageInfo.type) < AutomationConfig.PressingEfficacyThreshold)
            {
                return false;
            }
            // 护甲此前误读为 weapon.metaInfo，两条护甲资格分支全部失真
            var armor = agent.Equipment.armor?.metaInfo;
            var armorGrade = armor != null ? (int)armor.Grade : 0;

            if (agent.level > riskLevel)
            {
                return true;
            }
            else if (agent.level < riskLevel)
            {
                return false;
            }

            // 用异想体当前防御行（defenseId 可被核心抑制等脚本改写），而非固定 "1" 行
            var defense = creature.defense;
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

        public static bool IsRegionLocked(this AgentModel agent, EquipmentTypeInfo gift) => agent.Equipment.gifts.GetLockState(gift);

        public static void SetWaitingPassage(this AgentModel agent, PassageObjectModel passage = null)
        {
            if (passage == null)
            {
                var node = MapGraph.instance.GetSepiraNodeByRandom(agent.currentSefira);
                passage = node?.GetAttachedPassage();
            }
            if (passage == null)
            {
                return;
            }
            agent.SetWaitingPassage(passage);
            var passageObject = SefiraMapLayer.instance.GetPassageObject(passage);
            if (passageObject != null)
            {
                passageObject.OnPointEnter();
                passageObject.OnPointerClick();
            }
        }

        public static string Tag(this AgentModel agent) => $"<color=#66bfcd>{agent.name}</color>";

        public static int TotalStats(this AgentModel agent)
        {
            return Math.Min(WorkerPrimaryStat.MaxStatR(), (int)(agent.primaryStat.maxHP + agent.primaryStatExp.hp))
                   + Math.Min(WorkerPrimaryStat.MaxStatW(), (int)(agent.primaryStat.maxMental + agent.primaryStatExp.mental))
                   + Math.Min(WorkerPrimaryStat.MaxStatB(), (int)(agent.primaryStat.work + agent.primaryStatExp.work))
                   + Math.Min(WorkerPrimaryStat.MaxStatP(), (int)(agent.primaryStat.battle + agent.primaryStatExp.battle));
        }
    }
}