using AutoInority.Command;

namespace AutoInority.Posture
{
    /// <summary>
    /// 生产姿态：宏/种地每 tick 从 WorkingAgents/FarmingCreatures 无状态重建种子
    /// （替代旧 _repeatQueue 回炉续命），叠加应急反射——与旧 Main 全量行为等价。
    /// </summary>
    internal class ProductionPosture : PostureBase
    {
        public override void OnTick()
        {
            var automaton = Automaton.Instance;
            foreach (var command in automaton.WorkingAgents.Values)
            {
                automaton.Enqueue(command);
            }
            foreach (var creature in automaton.FarmingCreatures)
            {
                automaton.Enqueue(new FarmCommand(creature));
            }
            EnqueueThreats();
        }
    }
}
