using AutoInority.Creature;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 害羞门口弹回的真值表（mock 冒烟）：表情 0-4 → 是否弹回。
    /// 游戏侧伤害倍率 [0.5,1,1,1.5,2]、成功率加成 [+30,+10,0,-30,-50] 按表情：
    /// 0/1 友好放行，2/3/4 弹回待机。ViableAtDoor（门口复检）经
    /// ShyLookPolicy.ShouldBounce 固化本真值表。
    /// </summary>
    public class ShyLookBounceTest
    {
        [Theory]
        [InlineData(0, false)] // 伤害 0.5x、成功率 +30
        [InlineData(1, false)] // 伤害 1.0x、成功率 +10
        [InlineData(2, true)]  // 伤害 1.0x、成功率 ±0
        [InlineData(3, true)]  // 伤害 1.5x、成功率 -30
        [InlineData(4, true)]  // 伤害 2.0x、成功率 -50
        public void FaceTruthTable(int faceType, bool shouldBounce)
        {
            Assert.Equal(shouldBounce, ShyLookPolicy.ShouldBounce(faceType));
        }
    }
}
