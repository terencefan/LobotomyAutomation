namespace AutoInority.Creature
{
    public interface ICreatureExtension
    {
        bool AutoSuppress { get; }

        bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message);

        bool IsUrgent { get; }

        #region Confidence
        bool CheckConfidence(AgentModel agent, SkillTypeInfo skill);

        float GoodConfidence(AgentModel agent, SkillTypeInfo skill);

        float NormalConfidence(AgentModel agent, SkillTypeInfo skill);
        #endregion

        bool TryGetEGOGift(out EquipmentTypeInfo gift);

        SkillTypeInfo[] SkillSets();
    }
}