using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using AutoInority.Extentions;

using UnityEngine;

namespace AutoInority
{
    internal partial class Automaton
    {
        public const float FarmingAnimSpeed = 0.2f;

        public const float NormalAnimSpeed = 0.5f;

        internal HashSet<CreatureModel> FarmingCreatures { get; } = new HashSet<CreatureModel>();

        private static MethodInfo InitProcessText { get; } = typeof(IsolateRoom).GetMethod("InitProcessText", BindingFlags.NonPublic | BindingFlags.Instance);

        private static PropertyInfo IsWorkAllocated { get; } = typeof(IsolateRoom).GetProperty("IsWorkAllocated", BindingFlags.NonPublic | BindingFlags.Instance);

        private static PropertyInfo IsWorking { get; } = typeof(IsolateRoom).GetProperty("IsWorking", BindingFlags.NonPublic | BindingFlags.Instance);

        // 0 => "Gift_Hat",
        // 1 => "Gift_Eye",
        // 2 => "Gift_Mouth",
        // 3 => "Gift_Helmet",
        // 4 => "Gift_RightHand",
        // 5 => "Gift_Brooch",
        // 6 => "Gift_Ribborn",
        // 7 => "Gift_RightCheek",
        // 8 => "Gift_Face",
        // 9 => "Gift_BackRight",
        // 102 => "Gift_Mouth_replace",
        // 210 => "Gift_HeadBack",
        // 104 => "Gift_RightHand_replace",
        // 11 => "Gift_BackLeft",

        public void AssignWork(CreatureModel creature)
        {
            var agents = AgentManager.instance.GetAgentList().Where(x => x.IsAvailable());

            var candidates = Candidate.Suggest(agents, new[] { creature });
            candidates.Sort(Candidate.FarmComparer);

            Log.Info($"Found {agents.Count()} agents for {creature.Tag()}, candidates: {candidates.Count}");

            foreach (var candidate in candidates)
            {
                Log.Info(candidate.ToString());
            }

            foreach (var candidate in candidates)
            {
                if (candidate.Agent.IsAvailable() && candidate.Creature.IsAvailable())
                {
                    candidate.Apply();
                    return;
                }
            }
        }

        public void OnCancelWork(IsolateRoom room) => CancelFarm(room);

        public bool OnEnterRoom(IsolateRoom room, AgentModel worker, UseSkill skill)
        {
            var animator = room.CurrentWorkRoot.GetComponent<Animator>();
            animator.speed = NormalAnimSpeed; // set play speed back to normal

            if (FarmingCreatures.Where(x => x.Unit.room == room).Any())
            {
                var IsWorkAllocated = typeof(IsolateRoom).GetProperty("IsWorkAllocated", BindingFlags.NonPublic | BindingFlags.Instance);
                var IsWorking = typeof(IsolateRoom).GetProperty("IsWorking", BindingFlags.NonPublic | BindingFlags.Instance);
                var InitProcessText = typeof(IsolateRoom).GetMethod("InitProcessText", BindingFlags.NonPublic | BindingFlags.Instance);

                IsWorkAllocated.SetValue(room, false, null);
                IsWorking.SetValue(room, true, null);
                if (skill.skillTypeInfo.id != 5)
                {
                    room.DescController.Display(LocalizeTextDataModel.instance.GetText("WorkProcess_start_0"), -1);
                    room.StartWorkDesc();
                }
                InitProcessText.Invoke(room, new object[] { skill.skillTypeInfo.rwbpType });
                room.TurnOnRoomLight();
                return false;
            }
            return true;
        }

        public bool OnExitRoom(IsolateRoom room)
        {
            if (FarmingCreatures.Where(x => x.Unit.room == room).Any())
            {
                var animator = room.CurrentWorkRoot.GetComponent<Animator>();
                animator.speed = FarmingAnimSpeed;
                room.StopWorkDesc();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Enter farm mode
        /// </summary>
        /// <param name="creature"></param>
        public void ToggleFarming(CreatureModel creature)
        {
            var room = creature.Unit.room;

            if (FarmingCreatures.Remove(creature))
            {
                CancelFarm(room);
            }
            else
            {
                room.CurrentWorkRoot.SetActive(value: true);
                var animator = room.CurrentWorkRoot.GetComponent<Animator>();
                animator.speed = FarmingAnimSpeed;
                animator.SetTrigger("Run");
                room.TurnOnRoomLight();

                FarmingCreatures.Add(creature);
                var message = string.Format(Angela.Automaton.FarmOn, creature.Tag());
                Angela.Log(message);
            }
        }

        public void TryTrain(IEnumerable<AgentModel> agents)
        {
        }

        public void TryTrain()
        {
            var agents = AgentManager.instance.GetAgentList().Where(x => x.IsAvailable()).ToList();
            agents.Sort(Candidate.TrainComparer);

            foreach (var agent in agents)
            {
                if (agent.HasReachedExpLimit(out var types))
                {
                    continue;
                }

                var creatures = CreatureManager.instance.GetCreatureList().Where(x => x.IsCreature() && x.IsAvailable());
                var candidates = Candidate.Suggest(agent, creatures, types);
                candidates.Sort(Candidate.FarmComparer);

                foreach (var candidate in candidates)
                {
                    if (candidate.IsAvailable())
                    {
                        candidate.Apply();
                        return;
                    }
                }
            }
        }

        private void CancelFarm(IsolateRoom room)
        {
            FarmingCreatures.Remove(room.TargetUnit.model);
            var animator = room.CurrentWorkRoot.GetComponent<Animator>();
            animator.speed = NormalAnimSpeed; // set play speed back to normal
            animator.SetTrigger("Stop");
            room.TurnOffRoomLight();
            var message = string.Format(Angela.Automaton.FarmOff, room.TargetUnit.model.Tag());
            Angela.Log(message);
        }

        private bool TryFarm()
        {
            foreach (var creature in FarmingCreatures.Where(x => x.IsAvailable()))
            {
                var agents = AgentManager.instance.GetAgentList().Where(x => creature.GetExtension().FarmFilter(x));
                var candidates = Candidate.Suggest(agents, new[] { creature });
                candidates.Sort(Candidate.FarmComparer);

                foreach (var candidate in candidates)
                {
                    if (candidate.Agent.IsAvailable() && candidate.Creature.IsAvailable())
                    {
                        var workspaceNode = candidate.Creature.GetWorkspaceNode();
                        var passageNode = MapGraph.instance.GetNodeById(workspaceNode.GetId().Split('@')[0]);
                        candidate.Agent.SetWaitingPassage(passageNode.GetAttachedPassage());
                        candidate.Apply();
                        return true;
                    }
                }
            }
            return false;
        }
    }
}