namespace AutoInority.Creature
{
    internal class OldLadyExt : BaseCreatureExt
    {
        /// <summary>
        /// 计数低位（≤2/4）进调度优先队列：她唯一的回充手段是被工作
        /// （GOOD/NORM 结果 +1，OldLady.cs:61-67），而观察者机制随全设施工作
        /// 频率持续扣点（他者完成工作 -1，:81-87）；低位放着不管必出逃
        /// （≤0 进房的职员被施 buf 强制恐慌，:53-58）。≤2 而非 &lt;4 避免常驻占用。
        /// </summary>
        public override bool IsUrgent => QliphothCounter <= 2;

        public OldLadyExt(CreatureModel creature) : base(creature)
        {
        }
    }
}