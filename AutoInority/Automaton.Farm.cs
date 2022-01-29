using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using AutoInority.Command;
using AutoInority.Extentions;

using UnityEngine;

namespace AutoInority
{
    internal partial class Automaton
    {
        public const float FarmingAnimSpeed = 0.2f;

        public const float NormalAnimSpeed = 0.5f;

        public HashSet<CreatureModel> FarmingCreatures { get; } = new HashSet<CreatureModel>();

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

        public void OnCancelWork(IsolateRoom room) => TurnOffFarm(room);

        public bool OnEnterRoom(IsolateRoom room, AgentModel worker, UseSkill skill)
        {
            var animator = room.CurrentWorkRoot.GetComponent<Animator>();
            animator.speed = NormalAnimSpeed; // set play speed back to normal

            if (FarmingCreatures.Where(x => x.Unit.room == room).Any())
            {
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
            if (!creature.IsCreature())
            {
                return;
            }

            var room = creature.Unit.room;

            if (FarmingCreatures.Remove(creature))
            {
                TurnOffFarm(room);
            }
            else
            {
                Enqueue(new FarmCommand(creature));
                TurnOnFarm(room);
            }
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

        private void TurnOnFarm(IsolateRoom room)
        {
            var creature = room.TargetUnit.model;
            var animator = room.CurrentWorkRoot.GetComponent<Animator>();
            room.CurrentWorkRoot.SetActive(value: true);
            animator.speed = FarmingAnimSpeed;
            animator.SetTrigger("Run");
            room.TurnOnRoomLight();

            FarmingCreatures.Add(creature);
            var message = string.Format(Angela.Automaton.FarmOn, creature.Tag());
            Angela.Log(message);
        }

        private void TurnOffFarm(IsolateRoom room)
        {
            var creature = room.TargetUnit.model;
            var animator = room.CurrentWorkRoot.GetComponent<Animator>();
            animator.speed = NormalAnimSpeed; // set play speed back to normal
            animator.SetTrigger("Stop");
            room.TurnOffRoomLight();

            FarmingCreatures.Remove(creature);
            var message = string.Format(Angela.Automaton.FarmOff, creature.Tag());
            Angela.Log(message);
        }
    }
}