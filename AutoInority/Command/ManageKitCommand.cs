using System;
using System.Collections.Generic;
using System.Linq;

using AutoInority.Extentions;

namespace AutoInority.Command
{
    internal class ManageKitCommand : BaseCommand
    {
        private readonly CreatureModel _kit;

        public override bool IsCompleted => !_kit.isOverloaded;

        public override bool IsApplicable => _kit.IsAvailable();

        public ManageKitCommand(CreatureModel creature)
        {
            if (!creature.IsKit())
            {
                throw new Exception($"{creature.metaInfo.name} is not a creature");
            }
            _kit = creature;
        }

        public override bool Equals(ICommand other) => Equals(other as ManageKitCommand);

        public override bool Execute()
        {
            if (!_kit.IsAvailable())
            {
                return false;
            }
            var ext = _kit.GetKitExtension();
            var agents = AgentManager.instance.GetAgentList().Where(x => x.IsAvailable() && ext.CanUse(x)).ToList();

            if (agents.Count > 0)
            {
                agents.Sort(DistanceComparer);
                var agent = agents.First();
                agent.ManageKitCreature(_kit);
                agent.counterAttackEnabled = false;
                _kit.Unit.room.OnWorkAllocated(agent);
                Angela.Log($"{agent.Tag()} is working on {_kit.Tag()}");
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return -957138030 + EqualityComparer<CreatureModel>.Default.GetHashCode(_kit);
        }

        public override PriorityEnum Priority() => PriorityEnum.MANAGE_KIT;

        protected int DistanceComparer(AgentModel x, AgentModel y)
        {
            var d1 = Graph.Distance(x, _kit);
            var d2 = Graph.Distance(y, _kit);
            return d1.CompareTo(d2);
        }

        private bool Equals(ManageKitCommand other) => other != null && Equals(_kit, other._kit);
    }
}
