namespace AutoInority.Creature
{
    internal class PunishingBirdExt : BaseCreatureExt
    {
        public PunishingBirdExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool AutoSuppress => false;
    }
}