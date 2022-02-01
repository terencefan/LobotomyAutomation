using System;
using System.Collections.Generic;
using System.Linq;

using AutoInority.Extentions;

namespace AutoInority.Command
{
    internal class ManageCreatureCommand : BaseCommand
    {
        private readonly CreatureModel _creature;

        public override bool IsCompleted => !_creature.IsUrgent();

        public override bool IsApplicable => _creature.IsAvailable();

        public ManageCreatureCommand(CreatureModel creature)
        {
            if (!creature.IsCreature())
            {
                throw new Exception($"{creature.metaInfo.name} is not a creature");
            }
            _creature = creature;
        }

        public override bool Execute()
        {
            var ext = _creature.GetExtension();
            var agents = AgentManager.instance.GetAgentList().Where(x => x.IsAvailable());
            var candidates = Candidate.Suggest(agents, new[] { _creature });

            if (candidates.Any())
            {
                candidates.Sort(Candidate.ManageComparer);
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
                    return _creature.IsUrgent() ? PriorityEnum.MANAGE_CREATURE_1_URGENT : PriorityEnum.MANAGE_CREATURE_1;
                case 2:
                    return _creature.IsUrgent() ? PriorityEnum.MANAGE_CREATURE_2_URGENT : PriorityEnum.MANAGE_CREATURE_2;
                case 3:
                    return _creature.IsUrgent() ? PriorityEnum.MANAGE_CREATURE_3_URGENT : PriorityEnum.MANAGE_CREATURE_3;
                case 4:
                    return _creature.IsUrgent() ? PriorityEnum.MANAGE_CREATURE_4_URGENT : PriorityEnum.MANAGE_CREATURE_4;
                case 5:
                    return _creature.IsUrgent() ? PriorityEnum.MANAGE_CREATURE_5_URGENT : PriorityEnum.MANAGE_CREATURE_5;
                default:
                    return PriorityEnum.DEFAULT;
            }
        }

        public override bool Equals(ICommand other) => Equals(other as ManageCreatureCommand);

        public override int GetHashCode()
        {
            return 507986143 + EqualityComparer<CreatureModel>.Default.GetHashCode(_creature);
        }

        private bool Equals(ManageCreatureCommand other) => other != null && Equals(_creature, other._creature);
    }
}