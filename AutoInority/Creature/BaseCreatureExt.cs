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

        public virtual bool AutoSuppress => _creature.GetRiskLevel() < (int)RiskLevel.WAW;

        public virtual bool IsUrgent => false;

        public virtual SkillTypeInfo[] SkillSets { get; } = { Attachment, Insight, Instinct, Repression };

        protected float CreatureEscapeConfidence => AutomationConfig.CreatureEscapeConfidence;

        protected float DeadConfidence => AutomationConfig.DeadConfidence;

        protected bool IsFarming => Automaton.Instance.FarmingCreatures.Contains(_creature);

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
        /// 门检默认放行：无门口特殊风险的异想体无需处理，覆写以启用门口弹回。
        /// </summary>
        public virtual bool ViableAtDoor(AgentModel agent, SkillTypeInfo skill, out string message)
        {
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
                // 蕾蒂希娅的心：带心员工派往任何非蕾蒂希娅目标，5-10s 后死亡并生成蕾怪
                new BufExclusionRule.Rule(
                    () => agent.HasUnitBuf(UnitBufType.LITTLEWITCH_HEART),
                    () => Script is LittleWitch,
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
            var rejection = BufExclusionRule.Evaluate(BufRules(agent));
            if (rejection != null)
            {
                message = string.Format(rejection.Message, agent.Tag(), _creature.Tag(), skill.Tag());
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
            var attackMultiplifier = UnitModel.GetDmgMultiplierByEgoLevel(_creature.GetAttackLevel(), agent.GetDefenseLevel());
            var defenseMultiplifier = agent.defense.GetMultiplier(damageInfo.type);
            var bufDamageMultiplifier = agent.GetBufDamageMultiplier(_creature, damageInfo);
            var multiplier = attackMultiplifier * defenseMultiplifier * bufDamageMultiplifier;

            var minDamage = damageInfo.min * multiplier;
            var maxDamage = damageInfo.max * multiplier;

            switch (damageInfo.type)
            {
                case RwbpType.R:
                case RwbpType.B:
                case RwbpType.N:
                    return Confidence.Survive(agent.hp, minDamage, maxDamage, workSuccessProb, maxCubeCount) > AutomationConfig.DeadConfidence;
                case RwbpType.W:
                    return Confidence.Survive(MentalFix(agent.mental), minDamage, maxDamage, workSuccessProb, maxCubeCount) > AutomationConfig.DeadConfidence;
                case RwbpType.P:
                    minDamage *= agent.maxHp / 100f;
                    maxDamage *= agent.maxHp / 100f;
                    return Confidence.Survive(agent.hp, minDamage, maxDamage, workSuccessProb, maxCubeCount) > AutomationConfig.DeadConfidence;
            }
            return false;
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

        protected virtual float MentalFix(float mental) => mental;

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