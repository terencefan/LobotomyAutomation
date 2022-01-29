using System;
using System.Collections.Generic;
using System.Linq;

using AutoInority.Extentions;

namespace AutoInority.Command
{
    internal class SuppressCommand : BaseCommand
    {
        private readonly CreatureModel _creature;

        public override bool IsCompleted => _creature.state != CreatureState.ESCAPE;

        public override bool IsApplicable => _creature.state == CreatureState.ESCAPE;

        public SuppressCommand(CreatureModel creature)
        {
            if (!creature.IsCreature())
            {
                throw new Exception($"{creature.metaInfo.name} is not a creature");
            }
            _creature = creature;
        }

        public override bool Equals(ICommand other) => Equals(other as SuppressCommand);

        public override bool Execute()
        {
            if (!_creature.IsAvailable())
            {
                return false;
            }
            var agents = AgentManager.instance.GetAgentList().Where(x => x.IsAvailable() && x.IsCapableOfPressing(_creature)).ToList();

            if (agents.Count > 0)
            {
                agents.Sort(DistanceComparer);
                var agent = agents.First();
                agent.Suppress(_creature);
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return 507986143 + EqualityComparer<CreatureModel>.Default.GetHashCode(_creature);
        }

        public override PriorityEnum Priority()
        {
            switch (_creature.GetRiskLevel())
            {
                case 1:
                    return _creature is OrdealCreatureModel ? PriorityEnum.SUPRESS_ORDEAL_CREATURE_1 : PriorityEnum.SUPRESS_CREATURE_1;
                case 2:
                    return _creature is OrdealCreatureModel ? PriorityEnum.SUPRESS_ORDEAL_CREATURE_2 : PriorityEnum.SUPRESS_CREATURE_2;
                case 3:
                    return _creature is OrdealCreatureModel ? PriorityEnum.SUPRESS_ORDEAL_CREATURE_3 : PriorityEnum.SUPRESS_CREATURE_3;
                case 4:
                    return _creature is OrdealCreatureModel ? PriorityEnum.SUPRESS_ORDEAL_CREATURE_4 : PriorityEnum.SUPRESS_CREATURE_4;
                case 5:
                    return _creature is OrdealCreatureModel ? PriorityEnum.SUPRESS_ORDEAL_CREATURE_5 : PriorityEnum.SUPRESS_CREATURE_5;
                default:
                    return PriorityEnum.DEFAULT;
            }
        }

        protected int DistanceComparer(AgentModel x, AgentModel y)
        {
            var d1 = Graph.Distance(x, _creature);
            var d2 = Graph.Distance(y, _creature);
            return d1.CompareTo(d2);
        }

        private bool Equals(SuppressCommand other) => other != null && Equals(_creature, other._creature);
    }
}
