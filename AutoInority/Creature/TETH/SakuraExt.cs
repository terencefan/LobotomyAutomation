namespace AutoInority.Creature
{
    /// <summary>
    /// 樱下墓（O-04-100）：派工直连基类默认档，不经工厂兜底的 ExpectGoodAndNormalExt——
    /// 那一档会在 counter<=1 转 GOOD-only 保守闸，而樱下墓 GOOD/BAD 结果皆无后果
    /// （归零献祭由 ResolveAttract 自动救援兜底，救回/无候选均 Reset 计数回满，
    /// Sakura.cs:186-190/239-252），counter 不参与派工判定，仅保留员工生存闸。
    /// </summary>
    internal class SakuraExt : BaseCreatureExt
    {
        public SakuraExt(CreatureModel creature) : base(creature)
        {
        }

        /// <summary>
        /// 按游戏「连点 5 次唤醒」机制（Uncontrollable_Sakura._neededClicked=5）在一个
        /// 节流 tick 内打满阈值；WakeUp 冻结移动、2s 后走原版释放，唤醒后的多余点击为
        /// no-op。其余吸引类型保持每 tick 单点节奏，唯樱下墓「到达即献祭」需连点。
        /// </summary>
        internal static void ResolveAttract(WorkerModel worker)
        {
            if (worker.unconAction is Uncontrollable_Sakura)
            {
                for (var i = 0; i < 5; i++)
                {
                    worker.unconAction.OnClick();
                }
            }
        }
    }
}
