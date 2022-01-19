using System.Collections.Generic;
using System.Linq;

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

        public virtual bool AutoSuppress => false;

        public virtual bool IsUrgent => false;

        public virtual SkillTypeInfo[] SkillSets { get; } = All;

        protected static SkillTypeInfo[] All => new SkillTypeInfo[] { Instinct, Insight, Attachment, Repression };

        protected float CounterDecreaseConfidence => Automaton.Instance.CounterDecreaseConfidence;

        protected float CreatureEscapeConfidence => Automaton.Instance.CreatureEscapeConfidence;

        protected float DeadConfidence => Automaton.Instance.DeadConfidence;

        protected int QliphothCounter => _creature.qliphothCounter;

        public BaseCreatureExt(CreatureModel creature)
        {
            _creature = creature;
        }

        public virtual bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            message = null;
            if (agent.HasUnitBuf(UnitBufType.LITTLEWITCH_HEART) && !(_creature.script is LittleWitch))
            {
                message = string.Format(Angela.Creature.Laetitia, agent.Tag(), _creature.Tag(), skill.Tag());
                return false;
            }
            if (agent.GetUnitBufList().Where(x => x is FairyBuf).Any() && !(_creature.script is Fairy))
            {
                message = string.Format(Angela.Creature.Fairy, agent.Tag(), _creature.Tag(), skill.Tag());
                return false;
            }
            return true;
        }

        public virtual bool CheckConfidence(AgentModel agent, SkillTypeInfo skill) => CheckSurvive(agent, skill);

        public bool CheckSurvive(AgentModel agent, SkillTypeInfo skill)
        {
            var workSuccessProb = CalculateWorkSuccessProb(agent, skill);
            var maxCubeCount = _creature.metaInfo.feelingStateCubeBounds.GetLastBound();

            DamageInfo damageInfo = _creature.metaInfo.workDamage.Copy() * GetDamageMultiplierInWork(agent, skill);
            var attackMultiplifier = UnitModel.GetDmgMultiplierByEgoLevel(_creature.GetAttackLevel(), agent.GetDefenseLevel());
            var defenseMultiplifier = agent.defense.GetMultiplier(damageInfo.type);
            var multiplier = attackMultiplifier * defenseMultiplifier;

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

        public float GoodConfidence(AgentModel agent, SkillTypeInfo skill) => Confidence.InRange(_creature.MaxCube(), CalculateWorkSuccessProb(agent, skill), _creature.GoodBound());

        public float NormalConfidence(AgentModel agent, SkillTypeInfo skill) => Confidence.InRange(_creature.MaxCube(), CalculateWorkSuccessProb(agent, skill), _creature.NormalBound());

        public virtual bool TryGetEGOGift(out EquipmentTypeInfo gift)
        {
            gift = _creature.metaInfo.equipMakeInfos.Find((x) => x.equipTypeInfo.type == EquipmentTypeInfo.EquipmentType.SPECIAL)?.equipTypeInfo;
            return gift != null;
        }

        public IEnumerable<AgentModel> FindAgents()
        {
            throw new System.NotImplementedException();
        }

        protected virtual float CalculateWorkSuccessProb(AgentModel agent, SkillTypeInfo skill)
        {
            return _creature.CalculateWorkSuccessProb(agent, skill);
        }

        protected virtual bool CheckNormal(AgentModel agent, SkillTypeInfo skill)
        {
            var confidence = NormalConfidence(agent, skill);
            if (_creature.qliphothCounter > 1)
            {
                return confidence > Automaton.Instance.CounterDecreaseConfidence;
            }
            return confidence > Automaton.Instance.CreatureEscapeConfidence;
        }

        protected virtual float GetDamageMultiplierInWork(AgentModel agent, SkillTypeInfo skill)
        {
            var useSkill = FakeUseSkill(skill, agent);
            return _creature.script.GetDamageMultiplierInWork(useSkill);
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