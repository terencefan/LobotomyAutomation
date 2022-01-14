using Xunit;

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
            Assert.Equal(expected, Confidence.P(n, r, prob));
        }

        [Theory]
        [InlineData(100f, 6f, 0.7, 15, 1)]
        [InlineData(100f, 6f, 0.7, 20, 0.9999994)]
        [InlineData(100f, 6f, 0.6, 20, 0.9999526)]
        [InlineData(100f, 6f, 0.5, 25, 0.94612396)]
        [InlineData(100f, 6f, 0.4, 25, 0.7264691)]
        [InlineData(100f, 6f, 0.3, 25, 0.32307193)]
        [InlineData(100f, 7f, 0.7, 20, 0.999957)]
        [InlineData(100f, 8f, 0.7, 20, 0.9987211)]
        [InlineData(100f, 9f, 0.7, 20, 0.99486184)]
        public void SurviveConfidenceTest(float maxPoints, float damage, float prob, int count, float expected)
        {
            Assert.Equal(expected, Confidence.Survive(maxPoints, damage, prob, count));
        }

        [Theory]
        [InlineData(100f, 6f, 9f, 0.7, 10, 1)]
        [InlineData(100f, 6f, 9f, 0.7, 20, 0.9955558)]
        [InlineData(100f, 6f, 9f, 0.7, 30, 0.905271)]
        [InlineData(100f, 6f, 9f, 0.75, 30, 0.968413)]
        public void SurviveConfidenceRangeDamageTest(float maxPoints, float minDamage, float maxDamage, float prob, int count, float expected)
        {
            Assert.Equal(expected, Confidence.Survive(maxPoints, minDamage, maxDamage, prob, count));
        }

        [Theory]
        [InlineData(30, 0.5, 0, 0, 9.313226E-10)]
        [InlineData(30, 0.5, 30, -1, 9.313226E-10)]
        [InlineData(30, 0.5, 0, 18, 0.8997558)]
        [InlineData(30, 0.45, 13, 18, 0.6073775)]
        [InlineData(30, 0.5, 13, 18, 0.7189585)]
        [InlineData(30, 0.55, 13, 18, 0.6959234)]
        [InlineData(30, 0.6, 13, 18, 0.54766953)]
        [InlineData(30, 0.5, 13, 30, 0.81920266)]
        [InlineData(30, 0.5, 13, -1, 0.81920266)]
        public void RangeConfidenceTest(int count, float prob, int from, int to, float expected)
        {
            Assert.Equal(expected, Confidence.InRange(count, prob, from, to));
        }
    }
}
