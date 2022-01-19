using System.Reflection;

namespace AutoInority.Creature
{
    class SnowQueenExt : GoodNormalExt
    {
        private readonly FieldInfo _field;

        private bool IsFarming => Automaton.Instance.FarmingCreatures.Contains(_creature);

        public SnowQueenExt(CreatureModel creature) : base(creature)
        {
            _field = typeof(SnowQueen).GetField("_isBlockWork", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (IsUrgent())
            {
                if (agent.fortitudeLevel < 5)
                {
                    message = Message(Angela.Creature.SnowQueenDual, agent, skill);
                    return false;
                }
            }
            else if (IsFarming) // a tricky way to get her buff.
            {
                message = "";
                Log.Info("farming can work with");
                return skill.rwbpType == RwbpType.P;
            }
            else if (agent.Equipment.HasEquipment(1021) && GoodConfidence(agent, skill) < DeadConfidence)
            {
                message = Message(Angela.Creature.SnowQueen, agent, skill);
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }

        public override bool IsUrgent() => (bool)_field.GetValue(_creature.script);

        public override SkillTypeInfo[] SkillSets()
        {
            if (IsUrgent())
            {
                Log.Info("urgent");
                return new SkillTypeInfo[] { Instinct };
            }
            else if (IsFarming)
            {
                Log.Info("farming");
                return new SkillTypeInfo[] { Repression };
            }
            else
            {
                Log.Info("normal");
                return new SkillTypeInfo[] { Insight, Attachment };
            }
        }

        public override bool TryGetEGOGift(out EquipmentTypeInfo gift)
        {
            gift = EquipmentTypeList.instance.GetData(1023);
            return gift != null;
        }

        public override bool CheckConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            return IsFarming || base.CheckConfidence(agent, skill);
        }
    }
}
