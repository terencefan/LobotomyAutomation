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

        protected float CreatureEscapeConfidence => Automaton.Instance.CreatureEscapeConfidence;

        protected float DeadConfidence => Automaton.Instance.DeadConfidence;

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

        public virtual bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            // Laetitia
            if (!(Script is LittleWitch) && agent.HasUnitBuf(UnitBufType.LITTLEWITCH_HEART))
            {
                message = string.Format(Angela.Creature.Laetitia, agent.Tag(), _creature.Tag(), skill.Tag());
                return false;
            }

            // Fairy
            if (!(Script is Fairy) && agent.GetUnitBufList().Where(x => x is FairyBuf).Any())
            {
                message = string.Format(Angela.Creature.Fairy, agent.Tag(), _creature.Tag(), skill.Tag());
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
                    return Confidence.Survive(agent.hp, minDamage, maxDamage, workSuccessProb, maxCubeCount) > Automaton.Instance.DeadConfidence;
                case RwbpType.W:
                    return Confidence.Survive(MentalFix(agent.mental), minDamage, maxDamage, workSuccessProb, maxCubeCount) > Automaton.Instance.DeadConfidence;
                case RwbpType.P:
                    minDamage *= agent.maxHp / 100f;
                    maxDamage *= agent.maxHp / 100f;
                    return Confidence.Survive(agent.hp, minDamage, maxDamage, workSuccessProb, maxCubeCount) > Automaton.Instance.DeadConfidence;
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