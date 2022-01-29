namespace AutoInority.Creature
{
    public interface ICreatureKitExtension
    {
        bool CanUse(AgentModel agent);

        void Handle();

        void OnFixedUpdate();
    }
}