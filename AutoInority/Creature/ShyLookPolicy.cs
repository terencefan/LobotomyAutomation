namespace AutoInority.Creature
{
    /// <summary>
    /// 今天也很害羞的门口判定真值表（纯函数，供单测固化）：
    /// 表情 0/1 对工作友好（伤害 0.5x/1.0x、成功率 +30/+10、成功 tick 回血），
    /// 2/3/4（1.0x/1.5x/2.0x、+0/-30/-50）在门口弹回。
    /// </summary>
    internal static class ShyLookPolicy
    {
        internal static bool ShouldBounce(int faceType) => faceType > 1;
    }
}
