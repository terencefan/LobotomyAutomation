using System;
using System.Collections.Generic;
using System.Linq;

using AutoInority.Extentions;

namespace AutoInority.Command
{
    /// <summary>
    /// 训练命令：为单个员工找一份能刷其未满级维度的安全工作。需求长在人身上
    /// （经验缺口），活只是达成手段——「人找活」路径的命令化（原 TryTrain 内联体平移）。
    /// 门检照走全套：Suggest 内确定性预筛 + 置信度 + 存活卷积，不会为经验送人去死。
    /// 无状态重扫模型：IsCompleted 随员工可用性翻转，下 tick 由训练姿态重种子。
    /// </summary>
    internal class TrainCommand : BaseCommand
    {
        private readonly AgentModel _agent;

        public TrainCommand(AgentModel agent)
        {
            _agent = agent;
        }

        public override bool IsApplicable => _agent.IsAvailable() && !Automaton.InEmergency;

        public override bool IsCompleted => !_agent.IsAvailable();

        public override bool Equals(ICommand other) => Equals(other as TrainCommand);

        public override bool Execute()
        {
            if (!_agent.HasTrainableTypes(out var types))
            {
                return false;
            }

            var creatures = CreatureManager.instance.GetCreatureList().Where(x => x.IsCreature() && x.IsAvailable());
            var candidates = Candidate.Suggest(_agent, creatures, types);

            if (candidates.Any())
            {
                // 同级礼物效率优先（ExploitSufficientComparer 的省着用排序待 #141 合入后跟进）
                candidates.Sort(Candidate.ExploitComparer);
                var candidate = candidates.First();
                var workspaceNode = candidate.Creature.GetWorkspaceNode();
                var passageNode = MapGraph.instance.GetNodeById(workspaceNode.GetId().Split('@')[0]);
                _agent.SetWaitingPassage(passageNode.GetAttachedPassage());
                candidate.Apply();
                return true;
            }
            return false;
        }

        public override PriorityEnum Priority() => PriorityEnum.TRAIN_AGENT;

        public override int GetHashCode()
        {
            return 507986143 + EqualityComparer<AgentModel>.Default.GetHashCode(_agent);
        }

        private bool Equals(TrainCommand other) => other != null && Equals(_agent, other._agent);
    }
}
