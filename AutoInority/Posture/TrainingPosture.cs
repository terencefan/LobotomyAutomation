using System.Linq;

using AutoInority.Command;
using AutoInority.Extentions;

namespace AutoInority.Posture
{
    /// <summary>
    /// 训练姿态：经验缺口驱动（「人找活」）。每 tick 从可用且未满级的员工
    /// （TrainComparer 序：总属性升序，弱者优先拉平短板）为每人种一条
    /// TrainCommand，由优先级队列与镇压/管理命令公平仲裁。开发种子不跑，
    /// 状态保留，切回生产自动恢复。
    /// </summary>
    internal class TrainingPosture : PostureBase
    {
        public override void OnTick()
        {
            var automaton = Automaton.Instance;
            var agents = AgentManager.instance.GetAgentList().Where(x => x.IsAvailable()).ToList();
            agents.Sort(Candidate.TrainComparer);

            foreach (var agent in agents)
            {
                if (!agent.HasTrainableTypes(out _))
                {
                    continue; // 全满级者无经验缺口，不占队列位（Execute 内同门双保险）
                }
                automaton.Enqueue(new TrainCommand(agent));
            }
        }
    }
}
