using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using AutoInority.Extentions;

namespace AutoInority.Creature
{
    public abstract class BaseCreatureExt : ICreatureExtension
    {
        protected static readonly SkillTypeInfo Attachment = SkillTypeList.instance.GetData((long)RwbpType.B);

        protected static readonly SkillTypeInfo Insight = SkillTypeList.instance.GetData((long)RwbpType.W);

        protected static readonly SkillTypeInfo Instinct = SkillTypeList.instance.GetData((long)RwbpType.R);

        protected static readonly SkillTypeInfo Repression = SkillTypeList.instance.GetData((long)RwbpType.P);

        protected readonly CreatureModel _creature;

        /// <summary>
        /// 是否自动镇压出逃体：等级线（WAW 以下默认镇压）与游戏镇压试可性
        /// （CreatureBase.IsAutoSuppressable，如空虚之梦羊形态/肉食灯笼/惩戒鸟返回 false）
        /// 的合取。消费点每帧活查，形态翻转（羊→鸡）即时感知。
        /// </summary>
        public virtual bool AutoSuppress => _creature.GetRiskLevel() < (int)RiskLevel.WAW && (_creature.script?.IsAutoSuppressable() ?? true);

        public virtual bool IsUrgent => false;

        /// <summary>语义见 ICreatureExtension.AcceptsHeartAgent；默认拒绝。</summary>
        public virtual bool AcceptsHeartAgent => false;

        /// <summary>镇压扩展目标，默认无（语义见 ICreatureExtension.SuppressionTargets）。</summary>
        public virtual IEnumerable<CreatureModel> SuppressionTargets => new CreatureModel[0];

        public virtual SkillTypeInfo[] SkillSets { get; } = { Attachment, Insight, Instinct, Repression };

        protected float CreatureEscapeConfidence => AutomationConfig.CreatureEscapeConfidence;

        protected float DeadConfidence => AutomationConfig.DeadConfidence;

        protected bool IsExploiting => Automaton.Instance.ExploitingCreatures.Contains(_creature);

        protected bool IsOverloaded => _creature.isOverloaded;

        protected int QliphothCounter => _creature.qliphothCounter;

        protected CreatureBase Script => _creature.script;

        public BaseCreatureExt(CreatureModel creature)
        {
            _creature = creature;
        }

        protected BaseCreatureExt()
        {
        }

        /// <summary>
        /// 门检总入口（非虚）：全量门口规则与 creature 门检的组合，覆写无法绕过前者。
        /// 心之归属地兜底先行：游戏并不阻止手工把带心员工派往他处，只点燃 5~10s 引信
        /// （LittleWitchBuf.FixedUpdate），走完即员工死亡、女巫之子出笼；CanWorkWith
        /// 只约束自动化派工，手工误派唯有此线可拦。
        /// </summary>
        public bool ViableAtDoor(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (!RejectedByBufRules(agent, skill, out message))
            {
                return false;
            }
            return DoorCheck(agent, skill, out message);
        }

        /// <summary>
        /// creature 门检默认放行：无门口特殊风险的异想体无需处理，覆写以启用门口弹回。
        /// </summary>
        protected virtual bool DoorCheck(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            message = null;
            return true;
        }

        /// <summary>
        /// buf 危害排除表求值：命中且未豁免即拒绝并产出文案。派工闸（CanWorkWith）
        /// 与门口兜底（ViableAtDoor）共用同一张 BufRules 表，判定只有一份。
        /// </summary>
        private bool RejectedByBufRules(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            var rejection = BufExclusionRule.Evaluate(BufRules(agent));
            if (rejection != null)
            {
                message = string.Format(rejection.Message, agent.Tag(), _creature.Tag(), skill.Tag());
                return false;
            }
            message = null;
            return true;
        }

        /// <summary>
        /// buf 危害排除表：员工持有某 buf 时，派往（非豁免）目标会遭受工作之外的危害
        /// （死亡/处罚），故一律拒绝派工。新增 buf 危害 = 加一行表项；
        /// 求值语义（首命中即拒、豁免跳过、文案格式化）见 BufExclusionRule。
        /// 与技能型相关的拒绝（如碎甲）不适用此表，仍写在 CanWorkWith 内。
        /// </summary>
        private BufExclusionRule.Rule[] BufRules(AgentModel agent)
        {
            return new[]
            {
                // 蕾蒂希娅的心：带心员工派往任何非心之归属地目标，5-10s 后死亡并生成蕾怪；
                // 豁免=AcceptsHeartAgent（数据在 creature，LaetitiaExt 放行）
                new BufExclusionRule.Rule(
                    () => agent.HasUnitBuf(UnitBufType.LITTLEWITCH_HEART),
                    () => AcceptsHeartAgent,
                    Angela.Creature.Laetitia),
                // 精灵盛宴的祝福：游戏 FairyBuf 的击杀条件只看「派工在途」不看目标，
                // 重派回精灵本体同样走在途死亡窗口，故无豁免，8s 内禁派任何工作
                new BufExclusionRule.Rule(
                    () => agent.GetUnitBufList().Where(x => x is FairyBuf).Any(),
                    () => false,
                    Angela.Creature.Fairy),
            };
        }

        public virtual bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (!RejectedByBufRules(agent, skill, out message))
            {
                return false;
            }

            // Armor
            if (agent.HasEquipment(CrumblingArmorExt.GIFT))
            {
                if (skill.rwbpType == RwbpType.P)
                {
                    message = Message(Angela.Creature.ArmorWarning, agent, skill);
                    return false;
                }
                else if (skill.rwbpType == RwbpType.B)
                {
                    message = Message(Angela.Creature.ArmorKill, agent, skill);
                    return false;
                }
            }

            // GalaxyBoy
            var buf = agent.GetUnitBufByType(UnitBufType.FRIEND_TOKEN);
            if (buf != null && !(Script is GalaxyBoy))
            {
                var field = typeof(FriendTokenBuf).GetField("baseCreature", BindingFlags.NonPublic | BindingFlags.Instance);
                var boy = (GalaxyBoy)field.GetValue(buf);
                if (boy.model.qliphothCounter < 5)
                {
                    message = "";
                    return false;
                }
            }
            message = null;
            return true;
        }

        public bool CheckConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            return CheckWorkConfidence(agent, skill) && CheckSurvive(agent, skill);
        }

        public bool CheckSurvive(AgentModel agent, SkillTypeInfo skill)
        {
            var workSuccessProb = CalculateWorkSuccessProb(agent, skill);
            var maxCubeCount = _creature.metaInfo.feelingStateCubeBounds.GetLastBound();

            DamageInfo damageInfo = _creature.metaInfo.workDamage.Copy() * GetDamageMultiplierInWork(agent, skill);

            // 对齐游戏 WorkerModel.TakeDamage 的顺序：装备先修正伤害，再进入 EGO 等级 / buf / 抗性乘区。
            ApplyEquipmentDamageModifiers(agent, ref damageInfo);

            var attackMultiplifier = UnitModel.GetDmgMultiplierByEgoLevel(_creature.GetAttackLevel(), agent.GetDefenseLevel());
            var defenseMultiplifier = agent.defense.GetMultiplier(damageInfo.type);
            var bufDamageMultiplifier = agent.GetBufDamageMultiplier(_creature, damageInfo);
            var multiplier = attackMultiplifier * defenseMultiplifier * bufDamageMultiplifier;

            var minDamage = damageInfo.min * multiplier;
            var maxDamage = damageInfo.max * multiplier;

            // 游戏在 TakeDamage 各伤害分支先过 BarrierBuf.UseBarrier 抵扣再扣血（WorkerModel.cs:613/634/676/715 附近），
            // 此处把对应类型护盾的剩余抵扣量折算进有效血池。
            switch (damageInfo.type)
            {
                case RwbpType.R:
                case RwbpType.N:
                    return Confidence.Survive(agent.hp + agent.GetBarrierValue(damageInfo.type), minDamage, maxDamage, workSuccessProb, maxCubeCount) > AutomationConfig.DeadConfidence;
                case RwbpType.W:
                    return Confidence.Survive(agent.mental + agent.GetBarrierValue(RwbpType.W), minDamage, maxDamage, workSuccessProb, maxCubeCount) > AutomationConfig.DeadConfidence;
                case RwbpType.B:
                    // 游戏 B 伤害用同一抵扣后的 num2 同时扣 hp 与 mental，有效池取两者较小值；护盾只抵扣一次，等效两池各加 barrier(B)。
                    var pool = Math.Min(agent.hp, agent.mental) + agent.GetBarrierValue(RwbpType.B);
                    return Confidence.Survive(pool, minDamage, maxDamage, workSuccessProb, maxCubeCount) > AutomationConfig.DeadConfidence;
                case RwbpType.P:
                    minDamage *= agent.maxHp / 100f;
                    maxDamage *= agent.maxHp / 100f;
                    return Confidence.Survive(agent.hp + agent.GetBarrierValue(RwbpType.P), minDamage, maxDamage, workSuccessProb, maxCubeCount) > AutomationConfig.DeadConfidence;
            }
            return false;
        }

        // 复刻游戏 WorkerModel.TakeDamage 的装备修正层（armor -> weapon -> gifts，actor 传生物，与游戏一致）。
        // 装备 script 的 OnTakeDamage 可能带有真实副作用（反弹伤害、回血回精神、添加护盾 buff、写装备状态字段），
        // 而派工评估会在调度中反复触发（每 tick 重建候选），重复调用会污染游戏状态。
        // 因此只放行反编译排查后确认的"纯函数型"实现（只修改 ref dmg，无任何状态副作用），其余一律跳过：
        //   - DeathAngelArmor: 纯函数（按当前防御/buf 重算伤害并在小伤害时归零）；但同时持有白夜武器「失乐园」时会额外 RecoverHP/RecoverMental，此时跳过
        //   - NamelessFetusGift: 纯函数（dmg.min/max 各乘 0.95）
        //   - YinGift: 纯透传（不修改 dmg，调用无收益，跳过）
        //   - BlackSwanWeapon: 会 actor.TakeDamage 反弹真实伤害并播放特效，跳过
        //   - CensoredWeapon: 不修改 dmg，但写装备自身 hp_old/mp_old 状态字段，无收益，跳过
        //   - OneBadManyGoodArmor: 不修改 dmg，且按随机概率 RecoverMental，跳过
        //   - SlimeGirlArmor: 会真实添加 BarrierBuf 等 buf 并启动冷却计时器，跳过
        // 注：armor 的常规减伤（如 DeathAngelArmor.GetDefense）已由 agent.defense 聚合，包含在本类既有的抗性乘区中。
        private void ApplyEquipmentDamageModifiers(AgentModel agent, ref DamageInfo damageInfo)
        {
            var equipment = agent.Equipment;
            if (equipment == null)
            {
                return;
            }

            ApplyDeathAngelArmorModifier(agent, equipment, ref damageInfo);

            // weapon：现有实现均非纯函数，跳过。

            ApplyGiftsModifier(equipment, ref damageInfo);
        }

        /// <summary>
        /// 白夜装甲（DeathAngelArmor）修正：未持「失乐园」时其 OnTakeDamage 是纯函数；一旦同时持有
        /// 该武器，装甲会额外做小伤害归零并回血回精神（游戏 DeathAngelArmor.cs L43+），带副作用须跳过该组合。
        /// </summary>
        private void ApplyDeathAngelArmorModifier(AgentModel agent, UnitEquipSpace equipment, ref DamageInfo damageInfo)
        {
            if (equipment.armor != null && equipment.armor.script is DeathAngelArmor && !agent.HasEquipment((int)EquipmentId.ParadiseLost))
            {
                equipment.armor.OnTakeDamage(_creature, ref damageInfo);
            }
        }

        /// <summary>
        /// gifts 集合的饰品修正分发：白名单命中的饰品调 OnTakeDamage（当前唯一成员为
        /// NamelessFetusGift——dmg.min/max 各乘 0.95，纯函数重复评估无副作用）；
        /// 其余饰品带副作用（回精神/加 buf/写状态），一律跳过。
        /// </summary>
        private void ApplyGiftsModifier(UnitEquipSpace equipment, ref DamageInfo damageInfo)
        {
            var gifts = equipment.gifts;
            if (gifts == null)
            {
                return;
            }

            foreach (var gift in gifts.replacedGifts.Concat(gifts.addedGifts))
            {
                if (gift.script is NamelessFetusGift)
                {
                    gift.script.OnTakeDamage(_creature, ref damageInfo);
                }
            }
        }

        public virtual bool CheckWorkConfidence(AgentModel agent, SkillTypeInfo skill) => true;

        public virtual float ConfidenceMultiplifier(AgentModel agent, SkillTypeInfo skill) => 1;

        public virtual float GetConfidence(AgentModel agent, SkillTypeInfo skill) => GoodConfidence(agent, skill);

        public float GoodConfidence(AgentModel agent, SkillTypeInfo skill) => Confidence.InRange(_creature.MaxCube(), CalculateWorkSuccessProb(agent, skill), _creature.GoodBound() + 1);

        public float LessThanGoodConfidence(AgentModel agent, SkillTypeInfo skill) => Confidence.InRange(_creature.MaxCube(), CalculateWorkSuccessProb(agent, skill), 0, _creature.GoodBound());

        public float NormalConfidence(AgentModel agent, SkillTypeInfo skill) => Confidence.InRange(_creature.MaxCube(), CalculateWorkSuccessProb(agent, skill), _creature.NormalBound() + 1);

        public virtual bool TryGetEGOGift(out EquipmentTypeInfo gift)
        {
            gift = _creature.metaInfo.equipMakeInfos.Find((x) => x.equipTypeInfo.type == EquipmentTypeInfo.EquipmentType.SPECIAL)?.equipTypeInfo;
            return gift != null;
        }

        protected virtual float CalculateWorkSuccessProb(AgentModel agent, SkillTypeInfo skill)
        {
            Log.Debug(_creature.metaInfo.name);
            float prob = _creature.GetWorkSuccessProb(agent, skill);
            prob += _creature.GetObserveBonusProb() / 100f;
            prob += _creature.script.OnBonusWorkProb() / 100f;
            prob += agent.workProb / 500f;
            prob += agent.Equipment.GetWorkProbSpecialBonus(agent, skill) / 500f;

            if (agent.GetUnitBufList().Count > 0)
            {
                foreach (UnitBuf unitBuf in agent.GetUnitBufList())
                {
                    prob += unitBuf.GetWorkProbSpecialBonus(agent, skill) / 100f;
                }
            }

            prob = _creature.script.TranformWorkProb(prob);
            if (prob > 0.95f)
            {
                prob = 0.95f;
            }

            float num = _creature.GetRedusedWorkProbByCounter() / 100f;
            float num2 = _creature.ProbReductionValue / 100f;
            prob = !(num2 > 0f) ? prob - num : prob - num2;
            if (_creature.sefira.agentDeadPenaltyActivated)
            {
                prob -= 0.5f;
            }
            return prob > 0 ? prob : 0;
        }

        protected float CalculateWorkTime(AgentModel agent) => _creature.metaInfo.feelingStateCubeBounds.GetLastBound() / _creature.CalculateWorkSpeed(agent);

        protected virtual float GetDamageMultiplierInWork(AgentModel agent, SkillTypeInfo skill)
        {
            var useSkill = FakeUseSkill(skill, agent);
            return Script.GetDamageMultiplierInWork(useSkill);
        }

        protected string Message(string pattern, AgentModel agent, SkillTypeInfo skill)
        {
            return string.Format(pattern, agent.Tag(), _creature.Tag(), skill.Tag());
        }

        private UseSkill FakeUseSkill(SkillTypeInfo skillInfo, AgentModel agent)
        {
            UseSkill useSkill = new UseSkill();
            AgentUnit unit = agent.GetUnit();
            CreatureUnit unit2 = _creature.Unit;
            useSkill.room = unit2.room;
            useSkill.agent = agent;
            useSkill.agentView = unit;
            useSkill.targetCreature = _creature;
            useSkill.targetCreatureView = unit2;
            if (skillInfo.id != 5)
            {
                useSkill.workCount = 0;
                useSkill.maxCubeCount = _creature.metaInfo.feelingStateCubeBounds.GetLastBound();
                useSkill.workSpeed = _creature.GetCubeSpeed() * (1f + (_creature.GetObserveBonusSpeed() + agent.workSpeed) / 100f);
            }
            useSkill.startAgentHp = agent.hp;
            useSkill.startAgentMental = agent.mental;
            useSkill.skillTypeInfo = skillInfo;
            return useSkill;
        }
    }
}