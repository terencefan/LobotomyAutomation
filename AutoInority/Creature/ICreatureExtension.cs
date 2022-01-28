namespace AutoInority.Creature
{
    public interface ICreatureExtension
    {
        bool AutoSuppress { get; }

        bool IsUrgent { get; }

        SkillTypeInfo[] SkillSets { get; }

        bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message);

        bool CheckConfidence(AgentModel agent, SkillTypeInfo skill);

        float ConfidenceMultiplifier(AgentModel agent, SkillTypeInfo skill);

        bool FarmFilter(AgentModel agent);

        float GetConfidence(AgentModel agent, SkillTypeInfo skill);

        bool TryGetEGOGift(out EquipmentTypeInfo gift);
    }
}