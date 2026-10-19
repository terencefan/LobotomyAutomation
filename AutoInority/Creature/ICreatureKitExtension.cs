namespace AutoInority.Creature
{
    public interface ICreatureKitExtension
    {
        bool CanUse(AgentModel agent);

        /// <summary>为本 kit 挑一名使用者；无合适人选返回 null。默认按 IsAvailable
        /// 候选池（≥90% 血精）取最近者，kit 可覆写自己的选人标准（如避难所收伤员）。</summary>
        AgentModel SelectAgent();

        void OnFixedUpdate();
    }
}