namespace AutoInority.Creature
{
    internal class LuminousBraceletExt : EquipKitExt
    {
        public LuminousBraceletExt(CreatureModel kit) : base(kit)
        {
        }

        public override bool CanReturn(AgentModel agent)
        {
            return agent.hp == agent.maxHp;
        }
    }
}