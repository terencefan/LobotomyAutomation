using AutoInority.Extentions;

namespace AutoInority.Creature
{
    internal abstract class EquipKitExt : BaseKitExt
    {
        public EquipKitExt(CreatureModel kit) : base(kit)
        {
        }

        public abstract bool CanReturn(AgentModel agent);

        public override bool CanUse(AgentModel agent) => true;

        public override void OnFixedUpdate()
        {
            var agent = _kit.kitEquipOwner;
            if (agent != null && agent.IsAvailable())
            {
                if (CanReturn(agent))
                {
                    Angela.Log($"已安排{agent.Tag()}归还{_kit.Tag()}");
                    agent.ReturnKitCreature();
                }
                else
                {
                    agent.ReturnCancelKitCreature();
                }
            }
            base.OnFixedUpdate();
        }
    }
}