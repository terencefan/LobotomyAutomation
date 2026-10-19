using AutoInority.Creature;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 特蕾西娅持有计时：游戏规则为 channel &gt;30s 强制恐慌（Theresia.cs:51-54，
    /// mental=0），回复每 5s 一跳（:40-49）。CanStop 在 ≥25s 收手——吃满 5 跳
    /// （50 精神）并留 5s 边际；原 CanStop 恒真=秒断零收益。锚点：
    /// CreatureModel._currentSkill（私有，反射注）+ UseSkill._elapsedTime（私有，
    /// elapsedTime 为 get-only 属性）。
    /// </summary>
    public class TheresiaExtTest
    {
        private static TheresiaExt ExtWith(float elapsed)
        {
            var kit = GameBridge.Bare<CreatureModel>();
            var skill = GameBridge.Bare<UseSkill>();
            GameBridge.Set(skill, "_elapsedTime", elapsed);
            GameBridge.Set(kit, "_currentSkill", skill);
            return new TheresiaExt(kit);
        }

        [Theory]
        [InlineData(10f, false)]
        [InlineData(24.9f, false)]
        [InlineData(25f, true)]
        [InlineData(26f, true)]
        public void CanStopFollowsHoldThreshold(float elapsed, bool expected)
        {
            Assert.Equal(expected, ExtWith(elapsed).CanStop());
        }
    }
}
