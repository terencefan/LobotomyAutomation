using System.Collections.Generic;

namespace AutoInority.Creature
{
    public interface ICreatureExtension
    {
        bool AutoSuppress { get; }

        bool IsUrgent { get; }

        SkillTypeInfo[] SkillSets { get; }

        bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message);

        #region Confidence

        bool CheckConfidence(AgentModel agent, SkillTypeInfo skill);

        float ConfidencePenalty(AgentModel agent, SkillTypeInfo skill);

        float GoodConfidence(AgentModel agent, SkillTypeInfo skill);

        float NormalConfidence(AgentModel agent, SkillTypeInfo skill);

        #endregion Confidence

        bool FarmFilter(AgentModel agent);

        IEnumerable<AgentModel> FindAgents(int distance);

        bool TryGetEGOGift(out EquipmentTypeInfo gift);
    }
}