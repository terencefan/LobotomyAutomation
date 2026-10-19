namespace AutoInority.Creature
{
    internal class TheresiaExt : ChannelKitExt
    {
        // 特蕾西娅每 5s 回 10 精神，但游戏在 >30s 时强制恐慌（mental=0）。
        // 原 CanStop 恒真=秒断，安全却零收益；25s 收手吃满 5 跳并留 5s 边际
        // （CancelWork 与结算间的延迟）。
        private const float StopAt = 25f;

        public TheresiaExt(CreatureModel kit) : base(kit)
        {
        }

        // 调用契约：ChannelKitExt.OnFixedUpdate 已保证 currentSkill 非空才问 CanStop
        public override bool CanStop() => _kit.currentSkill.elapsedTime >= StopAt;
    }
}