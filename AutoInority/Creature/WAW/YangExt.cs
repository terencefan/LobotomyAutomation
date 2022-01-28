namespace AutoInority.Creature
{
    internal class YangExt : EquipKitExt
    {
        public YangExt(CreatureModel kit) : base(kit)
        {
        }

        public override bool CanReturn(AgentModel agent) => true;
    }
}
