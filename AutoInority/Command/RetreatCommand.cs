using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using AutoInority.Extentions;
using AutoInority.Posture;

namespace AutoInority.Command
{
    /// <summary>
    /// 撤离命令：压不动本部门威胁的员工显式撤离（战前避战），避免死亡。
    /// 每 tick 由 SuppressPosture 重新评估入队，排空即弃；IsApplicable 含幂等闸
    /// （已在撤往安全部门的路上不再发），优先级全表最高（保命先于一切派遣）。
    /// </summary>
    internal class RetreatCommand : BaseCommand
    {
        private readonly AgentModel _agent;

        // 反射锚点：AgentModel._currentWaitingPassage / PassageObjectModel.sefiraEnum（反编译验证）
        private static readonly FieldInfo WaitingPassageField = typeof(AgentModel).GetField("_currentWaitingPassage", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo PassageSefiraField = typeof(PassageObjectModel).GetField("sefiraEnum", BindingFlags.NonPublic | BindingFlags.Instance);

        public RetreatCommand(AgentModel agent)
        {
            _agent = agent;
        }

        public override bool IsCompleted => _agent.IsDead() || Automaton.Instance.CurrentPostureEnum != PostureEnum.SUPPRESS;

        public override bool IsApplicable
        {
            get
            {
                if (_agent.GetState() != AgentAIState.IDLE)
                {
                    return false;
                }
                var threats = GetThreatSefiraEnums();
                if (!threats.Contains(_agent.currentSefiraEnum))
                {
                    return false; // 已不在威胁部门
                }
                if (WaitingPassageField.GetValue(_agent) is PassageObjectModel waiting && PassageSefiraField.GetValue(waiting) is SefiraEnum target && !threats.Contains(target))
                {
                    return false; // 已在撤往安全部门的路上（幂等闸，防播报刷屏）
                }
                return true;
            }
        }

        public override bool Execute()
        {
            var passage = FindSafePassage();
            if (passage == null)
            {
                return false; // 无可去通道：原地不动（不炸）
            }
            RetreatToSafety(_agent, passage);
            Angela.Log(string.Format(Angela.Automaton.Retreat, _agent.Tag()));
            return true;
        }

        /// <summary>撤离落地：停动作 → 设等待通道 → 途中不自动接敌；镇压中低血撤退同用。</summary>
        internal static void RetreatToSafety(AgentModel agent, PassageObjectModel passage)
        {
            agent.StopAction();
            agent.SetWaitingPassage(passage);
            agent.counterAttackEnabled = false;
        }

        /// <summary>
        /// 安全通道：已开启部门中，到全部威胁部门最小图距最大者（同分取 sefiraList
        /// 靠前者，确定性防抖）；取其激活的横向通道，无则 null。
        /// </summary>
        internal static PassageObjectModel FindSafePassage()
        {
            var manager = SefiraManager.instance;
            var threats = GetThreatSefiraEnums();
            Sefira best = null;
            var bestScore = -1;
            foreach (var sefira in manager.sefiraList)
            {
                if (!manager.IsOpened(sefira.sefiraEnum))
                {
                    continue;
                }
                var score = ScoreSafety(sefira, threats);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = sefira;
                }
            }
            if (best == null)
            {
                return null;
            }
            return best.passageList.FirstOrDefault(x => x.isActivate && x.type == PassageType.HORIZONTAL);
        }

        /// <summary>安全分：到全部威胁部门的最小图距（无威胁为 0），纯函数可测。</summary>
        internal static int ScoreSafety(Sefira sefira, HashSet<SefiraEnum> threats)
        {
            return threats.Count == 0 ? 0 : threats.Min(x => Graph.Distance(x, sefira.sefiraEnum));
        }

        /// <summary>员工/收容物当前所在部门：逃逸节点反查（id @ 前段），失败退收容部门。</summary>
        internal static SefiraEnum SefiraOf(CreatureModel creature)
        {
            var target = creature.sefira != null ? creature.sefira.sefiraEnum : SefiraEnum.DUMMY;
            var node = creature.GetMovableNode()?.GetCurrentNode();
            if (node != null && SefiraManager.instance.GetSefiraByGenNodeId(node.GetId().Split('@')[0], out var sefira))
            {
                target = sefira.sefiraEnum;
            }
            return target;
        }

        /// <summary>威胁部门集：全部战斗目标当前所在部门。</summary>
        internal static HashSet<SefiraEnum> GetThreatSefiraEnums()
        {
            var result = new HashSet<SefiraEnum>();
            foreach (var creature in PostureBase.GetFightables())
            {
                var target = SefiraOf(creature);
                if (target != SefiraEnum.DUMMY)
                {
                    result.Add(target);
                }
            }
            return result;
        }

        public override PriorityEnum Priority() => PriorityEnum.RETREAT_AGENT;

        public override bool Equals(ICommand other) => Equals(other as RetreatCommand);

        public override int GetHashCode() => -440700910 + EqualityComparer<AgentModel>.Default.GetHashCode(_agent);

        private bool Equals(RetreatCommand other) => other != null && Equals(_agent, other._agent);
    }
}
