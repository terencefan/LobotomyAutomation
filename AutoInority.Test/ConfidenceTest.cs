using System;
using System.IO;

using Xunit;

namespace AutoInority.Test
{
    public class ConfidenceTest
    {
        [Theory]
        [InlineData(30, 0, 1)]
        [InlineData(30, 2, 435)]
        [InlineData(30, 15, 155117520)]
        [InlineData(32, 16, 601080390)]
        public void NCRTest(int n, int r, int expected)
        {
            Assert.Equal(expected, Confidence.NCR(n, r));
        }

        [Theory]
        [InlineData(30, 20, 0.7, 0.14156172)]
        [InlineData(30, 21, 0.7, 0.15729079)]
        [InlineData(30, 22, 0.7, 0.1501412)]
        [InlineData(30, 22, 0.75, 0.1593092)]
        [InlineData(30, 22, 0.8, 0.110558614)]
        [InlineData(30, 24, 0.8, 0.17945749)]
        public void PTest(int n, int r, float prob, float expected)
        {
            Assert.Equal(expected, Confidence.P(n, r, prob), 4);
        }

        [Fact]
        public void NCRTableBoundaryTest()
        {
            // 组合数表共 36 行（n ≤ 35）：n=35 合法，n=36 必须抛出而非越界取行
            Assert.Equal(4537567650L, Confidence.NCR(35, 17));
            Assert.Throws<ArgumentOutOfRangeException>(() => Confidence.NCR(36, 18));
        }

        [Fact]
        public void SurviveDeterministicDamageTest()
        {
            // (100, 9, 9, 0.7, 20)：δ=0 且第 11 次失败时 i·damage == maxPoints−1，
            // 恰好命中确定性存活边界。回归检测：旧实现此处 0/0 → NaN → 平台相关偏差。
            // 期望值用独立的二项累加（迭代组合数），不走被测代码
            const int n = 20;
            const int surviveFailures = 11;
            var failProb = 1f - 0.7f;
            var expected = 0.0;
            var ncr = 1L;
            for (int i = 0; i <= n; i++)
            {
                if (i > 0)
                {
                    ncr = ncr * (n - i + 1) / i;
                }
                if (i <= surviveFailures)
                {
                    expected += (float)(Math.Pow(failProb, i) * Math.Pow(0.7f, n - i) * ncr);
                }
            }
            var actual = Confidence.Survive(100, 9, 9, 0.7f, n);
            Assert.Equal(expected, actual, 6);
        }

        [Theory]
        [InlineData(100f, 6, 0.7, 15)]
        [InlineData(100f, 6, 0.7, 20)]
        [InlineData(100f, 6, 0.6, 20)]
        [InlineData(100f, 6, 0.5, 25)]
        [InlineData(100f, 6, 0.4, 25)]
        [InlineData(100f, 6, 0.3, 25)]
        [InlineData(100f, 7, 0.7, 20)]
        [InlineData(100f, 8, 0.7, 20)]
        [InlineData(100f, 9, 0.7, 20)]
        public void SurviveConfidenceTest(float maxPoints, int damage, float prob, int count)
        {
            RetryAssert((repeatTimes) =>
            {
                var expected = MonteCarlo(() => SurviveSimulate(maxPoints, damage, prob, count), repeatTimes);
                // min = max = damage：区间伤害退化为固定伤害
                var actual = Confidence.Survive(maxPoints, (double)damage, (double)damage, prob, count);
                Assert.InRange(actual, expected - 0.02, expected + 0.02);
            });
        }

        [Theory]
        [InlineData(100f, 4, 6, 0.5, 20)]
        [InlineData(150f, 9, 11, 0.5, 30)]
        [InlineData(150f, 8, 12, 0.5, 30)]
        [InlineData(150f, 7, 13, 0.5, 30)]
        [InlineData(150f, 6, 14, 0.5, 30)]
        [InlineData(150f, 1, 19, 0.5, 30)]
        [InlineData(100f, 6, 9, 0.2, 20)]
        [InlineData(100f, 6, 9, 0.4, 20)]
        [InlineData(100f, 6, 9, 0.6, 20)]
        [InlineData(100f, 6, 9, 0.8, 20)]
        [InlineData(100f, 6, 9, 0.2, 30)]
        [InlineData(100f, 6, 9, 0.4, 30)]
        [InlineData(100f, 6, 9, 0.6, 30)]
        [InlineData(100f, 6, 9, 0.8, 30)]
        [InlineData(100f, 5, 10, 0.5, 30)]
        [InlineData(100f, 4, 11, 0.5, 30)]
        [InlineData(100f, 3, 12, 0.5, 30)]
        [InlineData(100f, 2, 13, 0.5, 30)]
        [InlineData(100f, 1, 14, 0.5, 30)]
        // 混合区（i·min < M < i·max）针对性用例：min/max 差大、预期存活率在中间区域，
        // 专门校准 z 分数公式的符号与幅度（×2 bug 回归检测）
        [InlineData(120f, 2, 14, 0.5, 30)]
        [InlineData(120f, 3, 9, 0.6, 25)]
        [InlineData(200f, 5, 15, 0.4, 35)]
        [InlineData(100f, 4, 12, 0.5, 28)]
        public void SurviveConfidenceRangeDamageTest(float maxPoints, float minDamage, float maxDamage, float prob, int count)
        {
            RetryAssert((repeatTimes) =>
            {
                var expected = MonteCarlo(() => SurviveSimulate(maxPoints, minDamage, maxDamage, prob, count), repeatTimes);
                var actual = Confidence.Survive(maxPoints, minDamage, maxDamage, prob, count);
                Assert.InRange(actual, expected - 0.02, expected + 0.02);
            });
        }

        [Theory]
        [InlineData(30, 0.5, 0, 0)]
        [InlineData(30, 0.5, 30, -1)]
        [InlineData(30, 0.5, 0, 18)]
        [InlineData(30, 0.45, 13, 18)]
        [InlineData(30, 0.5, 13, 18)]
        [InlineData(30, 0.55, 13, 18)]
        [InlineData(30, 0.6, 13, 18)]
        [InlineData(30, 0.5, 13, 30)]
        [InlineData(30, 0.5, 13, -1)]
        public void RangeConfidenceTest(int count, float prob, int from, int to)
        {
            RetryAssert((repeatTimes) =>
            {
                var expected = MonteCarlo(() => InRangeSimulate(count, prob, from, to), repeatTimes);
                var actual = Confidence.InRange(count, prob, from, to);
                Assert.InRange(actual, expected - 0.01, expected + 0.01);
            });
        }

        [Theory]
        [InlineData(22, 0.45, 8, 15)]
        [InlineData(22, 0.5, 8, 15)]
        [InlineData(22, 0.52, 8, 15)]
        [InlineData(22, 0.53, 8, 15)]
        [InlineData(22, 0.55, 8, 15)]
        [InlineData(22, 0.6, 8, 15)]
        public void RangeConfidenceTestAlriune(int count, float prob, int from, int to)
        {
            RetryAssert((repeatTimes) =>
            {
                var expected = MonteCarlo(() => InRangeSimulate(count, prob, from, to), repeatTimes);
                var actual = Confidence.InRange(count, prob, from, to);
                Assert.InRange(actual, expected - 0.01, expected + 0.01);
            });
        }

        [Theory]
        [InlineData(30, 0.45, 13, 18)]
        [InlineData(30, 0.5, 13, 18)]
        [InlineData(30, 0.52, 13, 18)]
        [InlineData(30, 0.53, 13, 18)]
        [InlineData(30, 0.55, 13, 18)]
        [InlineData(30, 0.58, 13, 18)]
        [InlineData(30, 0.59, 13, 18)]
        [InlineData(30, 0.6, 13, 18)]
        public void RangeConfidenceTestSilent(int count, float prob, int from, int to)
        {
            RetryAssert((repeatTimes) =>
            {
                var expected = MonteCarlo(() => InRangeSimulate(count, prob, from, to), repeatTimes);
                var actual = Confidence.InRange(count, prob, from, to);
                Assert.InRange(actual, expected - 0.01, expected + 0.01);
            });
        }

        private void RetryAssert(Action<int> action)
        {
            int repeatTimes = 1 << 18;
            Exception latest = null;
            while (repeatTimes <= 1 << 22)
            {
                try
                {
                    action(repeatTimes);
                    return;
                }
                catch (Exception e)
                {
                    repeatTimes <<= 2;
                    latest = e;
                }
            }
            throw latest;
        }

        private double MonteCarlo(Func<bool> action, int repeatTimes)
        {
            int r = 0;
            for (int i = 0; i < repeatTimes; i++)
            {
                r += action() ? 1 : 0;
            }
            return (double)r / repeatTimes;
        }

        public bool InRangeSimulate(int count, float prob, int from, int to)
        {
            to = to < 0 ? count : to;
            var random = Random.Shared;
            var r = 0;
            for (int i = 0; i < count; i++)
            {
                if (random.NextDouble() < prob)
                {
                    r += 1;
                }
                if (r > to)
                {
                    return false;
                }
            }
            return r >= from;
        }

        private bool SurviveSimulate(float maxPoints, int damage, float prob, int count)
        {
            var random = Random.Shared;
            for (int i = 0; i < count; i++)
            {
                if (random.NextDouble() > prob)
                {
                    maxPoints -= damage;
                }
                if (maxPoints < 1)
                {
                    return false;
                }
            }
            return true;
        }

        private bool SurviveSimulate(float maxPoints, float minDamage, float maxDamage, float prob, int count)
        {
            var delta = maxDamage - minDamage;
            var random = Random.Shared;
            for (int i = 0; i < count; i++)
            {
                if (random.NextDouble() > prob)
                {
                    maxPoints -= ((float)random.NextDouble()) * delta + minDamage;
                }
                if (maxPoints <= 1)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
