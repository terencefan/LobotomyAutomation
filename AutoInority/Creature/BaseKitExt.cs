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

        public virtual AgentModel SelectAgent()
        {
            foreach (var agent in FindAgents())
            {
                return agent;
            }
            return null;
        }

        public virtual bool WantsDispatch() => false;

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

        /// <summary>员工到本 kit 的距离（测试可覆写注入假值，Graph.Distance 裸对象不安全）。</summary>
        protected virtual float Distance(AgentModel agent) => Graph.Distance(agent, _kit);

        protected int AgentComparer(AgentModel x, AgentModel y)
        {
            return Distance(x).CompareTo(Distance(y));
        }
    }
}