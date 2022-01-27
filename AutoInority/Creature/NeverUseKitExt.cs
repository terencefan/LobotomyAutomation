namespace AutoInority.Creature
{
    internal class NeverUseKitExt : BaseKitExt
    {
        public NeverUseKitExt(CreatureModel kit) : base(kit)
        {
        }

        public override bool CanUse(AgentModel agent) => false;
    }
}