using System.Collections.Generic;
using System.Linq;

using AutoInority.Command;
using AutoInority.Extentions;

namespace AutoInority.Posture
{
    /// <summary>
    /// 镇压姿态：生产休眠（不 seed 宏/种地），专注镇压与紧急应对。
    /// 人员分级：强（能压制本部门威胁）主动镇压，弱显式撤离（压不动也打不动的站
    /// 在原地只会送死）——强弱按本部门威胁判定，安全部门的员工不动。
    /// </summary>
    internal class SuppressPosture : PostureBase
    {
        public override void OnTick()
        {
            EnqueueThreats();
            EnqueueRetreats();
        }

        /// <summary>撤离种子：仅 IDLE 员工（工作中/镇压中/失控不打断，战中撤退由 AgentOnFixedUpdate 负责）。</summary>
        private static void EnqueueRetreats()
        {
            var fightables = new List<CreatureModel>(GetFightables());
            if (fightables.Count == 0)
            {
                return;
            }

            // 战斗目标按当前所在部门分桶，逐员工对本部门威胁做强弱判定
            var localThreats = new Dictionary<SefiraEnum, List<CreatureModel>>();
            foreach (var creature in fightables)
            {
                var target = RetreatCommand.SefiraOf(creature);
                if (target == SefiraEnum.DUMMY)
                {
                    continue;
                }
                if (!localThreats.TryGetValue(target, out var bucket))
                {
                    bucket = new List<CreatureModel>();
                    localThreats[target] = bucket;
                }
                bucket.Add(creature);
            }

            foreach (var agent in AgentManager.instance.GetAgentList())
            {
                if (agent.GetState() != AgentAIState.IDLE || !localThreats.TryGetValue(agent.currentSefiraEnum, out var threats))
                {
                    continue; // 非 IDLE 不打断；安全部门不撤
                }
                if (threats.Any(x => agent.IsCapableOfPressing(x)))
                {
                    continue; // 本地强者留下，SuppressCommand 同部门派遣会接上
                }
                Automaton.Instance.Enqueue(new RetreatCommand(agent));
            }
        }
    }
}
