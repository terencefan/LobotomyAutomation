using System.Collections.Generic;
using System.Linq;

using AutoInority.Extentions;

namespace AutoInority.Command
{
    internal class FarmCommand : BaseCommand
    {
        private readonly CreatureModel _creature;

        public override bool IsCompleted => !Automaton.Instance.FarmingCreatures.Contains(_creature);

        public override bool IsApplicable => _creature.IsAvailable() && !Automaton.InEmergency;

        public FarmCommand(CreatureModel creature)
        {
            _creature = creature;
        }

        public override bool Execute()
        {
            var agents = AgentManager.instance.GetAgentList().Where(x => x.IsAvailable() && _creature.GetExtension().FarmFilter(x));
            var candidates = Candidate.Suggest(agents, new[] { _creature });

            if (candidates.Any())
            {
                candidates.Sort(Candidate.FarmComparer);
                var candidate = candidates.First();
                var workspaceNode = candidate.Creature.GetWorkspaceNode();
                var passageNode = MapGraph.instance.GetNodeById(workspaceNode.GetId().Split('@')[0]);
                candidate.Agent.SetWaitingPassage(passageNode.GetAttachedPassage());
                candidate.Apply();
                return true;
            }
            return false;
        }

        public override PriorityEnum Priority()
        {
            switch (_creature.GetRiskLevel())
            {
                case 1:
                    return PriorityEnum.FARM_CREATURE_1;
                case 2:
                    return PriorityEnum.FARM_CREATURE_2;
                case 3:
                    return PriorityEnum.FARM_CREATURE_3;
                case 4:
                    return PriorityEnum.FARM_CREATURE_4;
                case 5:
                    return PriorityEnum.FARM_CREATURE_5;
                default:
                    return PriorityEnum.DEFAULT;
            }
        }

        public override bool Equals(ICommand other) => Equals(other as FarmCommand);

        public override int GetHashCode()
        {
            return 507986143 + EqualityComparer<CreatureModel>.Default.GetHashCode(_creature);
        }

        private bool Equals(FarmCommand other) => other != null && Equals(_creature, other._creature);
    }
}