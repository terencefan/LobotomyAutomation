namespace AutoInority.Creature
{
    internal class GalaxyBoyExt : BaseCreatureExt
    {
        public override bool IsUrgent => QliphothCounter < 5;

        public GalaxyBoyExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CheckWorkConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            // counter==0 时任何工作完工都会先 +1 并发新 token（GalaxyBoy.cs L92-95）：
            // 自救路径，解除受福者全灭后（BlessedCount>=3 且场上无 token 持有者）的软死锁
            return agent.HasUnitBuf(UnitBufType.FRIEND_TOKEN) || CenterBrain.BlessedCount < 3 || QliphothCounter == 0;
        }
    }
}