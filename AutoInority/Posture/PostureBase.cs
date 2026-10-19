namespace AutoInority.Posture
{
    /// <summary>
    /// 调度姿态基类：策略切换不清开发目标状态（ExploitingCreatures 挂在
    /// Automaton 上，切回自动恢复）。OnTick 只负责向 Automaton 入队，执行段在
    /// Automaton.Main 共用；OnEnter/OnExit 是二期事件驱动自动切换的扩展缝。
    /// </summary>
    internal abstract class PostureBase
    {
        public abstract void OnTick();

        public virtual void OnEnter()
        {
        }

        public virtual void OnExit()
        {
        }
    }
}
