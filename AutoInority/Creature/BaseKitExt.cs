using System.Collections.Generic;
using System.Linq;

using AutoInority.Extentions;

namespace AutoInority.Creature
{
    internal abstract class BaseKitExt : ICreatureKitExtension
    {
        protected readonly CreatureModel _kit;

        protected CreatureBase Script => _kit.script;

        public BaseKitExt(CreatureModel kit)
        {
            _kit = kit;
        }

        public virtual bool CanUse(AgentModel agent) => false;

        public IEnumerable<AgentModel> FindAgents()
        {
            var agents = AgentManager.instance.GetAgentList().Where(x => x.IsAvailable() && CanUse(x)).ToList();
            agents.Sort(AgentComparer);
            return agents;
        }

        public virtual void Handle()
        {
            var agents = FindAgents().ToList();
            if (agents.Any())
            {
                var agent = agents.First();
                agent.ManageKitCreature(_kit);
                agent.counterAttackEnabled = false;
                _kit.Unit.room.OnWorkAllocated(agent);
                Angela.Log($"已安排{agent.Tag()}使用{_kit.Tag()}");
                return;
            }
        }

        public virtual void OnFixedUpdate()
        {
        }

        protected int AgentComparer(AgentModel x, AgentModel y)
        {
            var d1 = Graph.Distance(x, _kit);
            var d2 = Graph.Distance(y, _kit);
            return d1.CompareTo(d2);
        }
    }
}