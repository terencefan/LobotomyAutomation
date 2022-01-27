namespace AutoInority.Creature
{
    internal class PunishingBirdExt : BaseCreatureExt
    {
        public override bool AutoSuppress => false;

        public PunishingBirdExt(CreatureModel creature) : base(creature)
        {
        }
    }
}