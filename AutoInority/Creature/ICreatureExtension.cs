namespace AutoInority.Creature
{
    public interface ICreatureExtension
    {
        bool AutoSuppress { get; }

        bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message);

        bool CheckConfidence(AgentModel agent, SkillTypeInfo skill);

        float GoodConfidence(AgentModel agent, SkillTypeInfo skill);

        bool IsUrgent();

        float NormalConfidence(AgentModel agent, SkillTypeInfo skill);

        bool TryGetEGOGift(out EquipmentTypeInfo gift);

        SkillTypeInfo[] SkillSets();
    }
}