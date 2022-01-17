using System;

using Xunit;
using Xunit.Sdk;

namespace AutoInority.Test
{
    public class UtilityTest
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
                var actual = Confidence.Survive(maxPoints, damage, prob, count);
                Assert.Equal(expected, actual, 2);
            });
        }

        [Theory]
        [InlineData(100f, 4, 11, 0.7, 30)]
        [InlineData(100f, 5, 10, 0.7, 30)]
        [InlineData(100f, 6, 9, 0.7, 30)]
        [InlineData(100f, 7, 8, 0.7, 30)]
        [InlineData(100f, 6, 9, 0.6, 30)]
        [InlineData(100f, 6, 9, 0.65, 30)]
        [InlineData(100f, 6, 9, 0.75, 30)]
        [InlineData(100f, 6, 9, 0.8, 30)]
        public void SurviveConfidenceRangeDamageTest(float maxPoints, int minDamage, int maxDamage, float prob, int count)
        {
            RetryAssert((repeatTimes) =>
            {
                var expected = MonteCarlo(() => SurviveSimulate(maxPoints, minDamage, maxDamage, prob, count), repeatTimes);
                var actual = Confidence.Survive(maxPoints, minDamage, maxDamage, prob, count);
                Assert.Equal(expected, actual, 2);
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
                Assert.Equal(expected, actual, 2);
            });
        }

        private void RetryAssert(Action<int> action)
        {
            int repeatTimes = 1 << 18;
            Exception latest = null;
            while (repeatTimes <= 1 << 24)
            {
                try
                {
                    action(repeatTimes);
                    return;
                }
                catch (AssertActualExpectedException e)
                {
                    repeatTimes <<= 3;
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
            var random = new Random();
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
            var random = new Random();
            for (int i = 0; i < count; i++)
            {
                if (random.NextDouble() > prob)
                {
                    maxPoints -= damage;
                }
                if (maxPoints <= 1)
                {
                    return false;
                }
            }
            return true;
        }

        private bool SurviveSimulate(float maxPoints, int minDamage, int maxDamage, float prob, int count)
        {
            var random = new Random();
            for (int i = 0; i < count; i++)
            {
                if (random.NextDouble() > prob)
                {
                    maxPoints -= random.Next(minDamage, maxDamage + 1);
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
