namespace AutoInority.Creature
{
    public interface ICreatureExtension
    {
        bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message);

        bool CheckConfidence(AgentModel agent, SkillTypeInfo skill);

        float GoodConfidence(AgentModel agent, SkillTypeInfo skill);

        float NormalConfidence(AgentModel agent, SkillTypeInfo skill);

        SkillTypeInfo[] SkillSets();

        bool IsUrgent();
    }
}