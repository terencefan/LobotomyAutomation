using AutoInority.Command;

namespace AutoInority.Posture
{
    /// <summary>
    /// 生产姿态：开发目标每 tick 从 ExploitingCreatures 无状态重建种子
    /// （替代旧 _repeatQueue 回炉续命）。应急反射是三姿态公共基座（Main 统一扫描）。
    /// </summary>
    internal class ProductionPosture : PostureBase
    {
        public override void OnTick()
        {
            var automaton = Automaton.Instance;
            foreach (var creature in automaton.ExploitingCreatures)
            {
                automaton.Enqueue(new ExploitCommand(creature));
            }
        }
    }
}
