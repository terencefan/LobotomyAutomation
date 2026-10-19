using System;
using System.Collections.Generic;
using System.Linq;

using AutoInority.Creature;

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
            var agent = _kit.GetKitExtension().SelectAgent();

            if (agent != null)
            {
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

        private bool Equals(ManageKitCommand other) => other != null && Equals(_kit, other._kit);
    }
}