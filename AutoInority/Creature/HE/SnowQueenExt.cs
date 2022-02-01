using System.Reflection;

namespace AutoInority.Creature
{
    internal class SnowQueenExt : BaseCreatureExt
    {
        public static readonly int DummyGiftId = 1021;

        private static readonly int FireBirdArmor = 300061;

        private static readonly int RealGiftId = 1023;

        private readonly FieldInfo _field;

        public override bool IsUrgent => IsFreezing;

        public override SkillTypeInfo[] SkillSets
        {
            get
            {
                if (IsFreezing)
                {
                    return new SkillTypeInfo[] { Instinct };
                }
                else if (IsFarming)
                {
                    return new SkillTypeInfo[] { Repression };
                }
                else
                {
                    return new SkillTypeInfo[] { Insight, Attachment };
                }
            }
        }

        private bool IsFreezing => (bool)_field.GetValue(_creature.script);

        public SnowQueenExt(CreatureModel creature) : base(creature)
        {
            _field = typeof(SnowQueen).GetField("_isBlockWork", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (agent.HasEquipment(FireBirdArmor))
            {
                message = Message(Angela.Creature.SnowQueenFire, agent, skill);
                return false;
            }
            else if (IsFreezing)
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
                return skill.rwbpType == RwbpType.P;
            }
            else if (agent.HasEquipment(DummyGiftId) && GoodConfidence(agent, skill) < DeadConfidence)
            {
                message = Message(Angela.Creature.SnowQueen, agent, skill);
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }

        public override bool TryGetEGOGift(out EquipmentTypeInfo gift)
        {
            gift = EquipmentTypeList.instance.GetData(RealGiftId);
            return gift != null;
        }
    }
}