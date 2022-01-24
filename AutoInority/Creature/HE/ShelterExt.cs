namespace AutoInority.Creature
{
    internal class ShelterExt : ChannelKitExt
    {
        public ShelterExt(CreatureModel kit) : base(kit)
        {
        }

        public override bool CanStop() => true;
    }
}