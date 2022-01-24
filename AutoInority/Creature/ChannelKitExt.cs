using AutoInority.Extentions;

namespace AutoInority.Creature
{
    internal abstract class ChannelKitExt : BaseKitExt
    {
        public ChannelKitExt(CreatureModel kit) : base(kit)
        {
        }

        public abstract bool CanStop();

        public override bool CanUse(AgentModel agent) => true;

        public override void OnFixedUpdate()
        {
            if (_kit.currentSkill != null && CanStop())
            {
                Angela.Log($"{_kit.currentSkill.agent.Tag()}已停止使用{_kit.Tag()}");
                _kit.currentSkill.CancelWork();
            }
            base.OnFixedUpdate();
        }
    }
}