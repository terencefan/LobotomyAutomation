namespace AutoInority
{
    public static class AgentModelExtensions
    {
        public static bool Available(this AgentModel agent)
        {
            return agent.hp == agent.maxHp && agent.mental == agent.maxMental && agent.GetState() == AgentAIState.IDLE;
        }

        public static bool CanWorkWith(this AgentModel agent, CreatureModel creature, SkillTypeInfo skill)
        {
            return Common.CanWorkWithCreature(agent, creature, skill) && agent.Available() && creature.Available();
        }

        public static bool HasEGOGift(this AgentModel agent, CreatureModel creature, out CreatureEquipmentMakeInfo gift)
        {
            if (creature.TryGetEGOGift(out gift))
            {
                return agent.Equipment.gifts.HasEquipment(gift.equipTypeInfo.id);
            }
            return true;
        }

        public static string Tag(this AgentModel agent) => $"<color=#66bfcd>{agent.name}</color>";
    }
}
