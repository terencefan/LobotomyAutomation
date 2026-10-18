using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 今天也很害羞（ShyLook）质量闸门的可达性数学。
    ///
    /// 游戏数据：MaxCube=12（feelingStateCubeBounds=[6,9,12]），
    /// NormalBound()=6 → NormalConfidence = P(X≥7)，X~B(12,p)；
    /// 成功概率 p 经 CalculateWorkSuccessProb 上限 0.95 封顶。
    ///
    /// 闸门（ExpectNormalExt，ShyThing 无逆卡巴拉计数器走 0.99 分支）：
    /// NormalConfidence > 0.99 ⟺ P(X≥7 | B(12,p)) > 0.99 ⟺ p ≳ 0.826。
    ///
    /// 表情成功率加成 [+30,+10,0,-30,-50]/100，基准 0.4~0.5，强干员再加
    /// workProb/500（约 +0.04~0.06）与观察加成（约 +0.05）：
    /// - 表情 0 + 强干员：p≈0.84~0.95 → 过闸（弱干员 p≈0.75~0.8 过不了）
    /// - 表情 1：p≤0.71 → 过不了闸
    /// - 表情 2/3/4：p≤0.55 → 被 CanWorkWith 的表情门槛先行拒绝
    /// 固化这组数字，防止日后误以为 0.99 闸对 12 箱异想体「不可达」或「形同虚设」。
    /// </summary>
    public class ShyLookGateTest
    {
        [Theory]
        [InlineData(0.95f, true)]  // 封顶：P(X≥7)≈1.0
        [InlineData(0.90f, true)]  // P(X≥7)≈0.9995
        [InlineData(0.83f, true)]  // 临界之上（阈值 p≈0.8254）
        [InlineData(0.82f, false)] // 临界之下
        [InlineData(0.80f, false)] // P(X≥7)≈0.9806
        [InlineData(0.71f, false)] // 表情 1 + 强干员的理论上限
        public void NormalConfidenceGateReachability(float prob, bool passesGate)
        {
            var confidence = Confidence.InRange(12, prob, 6 + 1);
            var passes = confidence > 0.99f;
            if (passesGate)
            {
                Assert.True(passes, $"p={prob}: P(X>=7)={confidence} 应过 0.99 闸");
            }
            else
            {
                Assert.True(!passes, $"p={prob}: P(X>=7)={confidence} 不应过 0.99 闸");
            }
        }

        [Fact]
        public void NormalConfidenceAtCapExceedsGate()
        {
            // 0.95 封顶时闸门必须可达——若此断言失败，说明 ShyThing 会被饿死成永不工作
            Assert.True(Confidence.InRange(12, 0.95f, 6 + 1) > 0.99f);
        }

        [Fact]
        public void GoodConfidenceGateUnreachable()
        {
            // 对照：GoodConfidence 闸（P(X≥10)>0.99）在 0.95 封顶下不可达（≈0.9804），
            // 这就是 ShyThing 不能用 ExpectGoodAndNormalExt 的 0.99 GOOD 分支的原因
            Assert.True(Confidence.InRange(12, 0.95f, 9 + 1) <= 0.99f);
        }
    }
}
