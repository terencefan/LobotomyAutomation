using AutoInority.Creature;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// buf 危害排除表的求值语义（BufExclusionRule.Evaluate）真值表：
    /// 首个「命中且未豁免」的规则被返回（文案模板透传、格式化在调用方），
    /// 豁免行跳过，全不命中放行。
    /// 行为锚点：BaseCreatureExt.CanWorkWith 经此求值心之/精灵祝福两行。
    /// </summary>
    public class BufExclusionTest
    {
        private static BufExclusionRule.Rule Rule(bool match, bool exempt, string message)
        {
            return new BufExclusionRule.Rule(() => match, () => exempt, message);
        }

        [Fact]
        public void EmptyTableAllows()
        {
            Assert.Null(BufExclusionRule.Evaluate(new BufExclusionRule.Rule[0]));
        }

        [Fact]
        public void NoMatchAllows()
        {
            var rules = new[] { Rule(match: false, exempt: false, message: "{0}x") };
            Assert.Null(BufExclusionRule.Evaluate(rules));
        }

        [Fact]
        public void FirstMatchRejects()
        {
            var rules = new[]
            {
                Rule(match: true, exempt: false, message: "first {0}"),
                Rule(match: true, exempt: false, message: "second {0}"),
            };
            Assert.Same(rules[0], BufExclusionRule.Evaluate(rules));
        }

        [Fact]
        public void ExemptRowSkippedFallsThrough()
        {
            var rules = new[]
            {
                Rule(match: true, exempt: true, message: "exempted"),
                Rule(match: true, exempt: false, message: "hit {1}"),
            };
            Assert.Same(rules[1], BufExclusionRule.Evaluate(rules));
        }

        [Fact]
        public void MessageFormattedByPosition()
        {
            var rules = new[] { Rule(match: true, exempt: false, message: "{0}/{1}/{2}") };
            Assert.Equal("{0}/{1}/{2}", BufExclusionRule.Evaluate(rules).Message);
        }

        [Fact]
        public void MatchShortCircuitsExempt()
        {
            var evaluated = false;
            var rules = new[]
            {
                new BufExclusionRule.Rule(() => false, () => { evaluated = true; return false; }, "x"),
            };
            Assert.Null(BufExclusionRule.Evaluate(rules));
            Assert.False(evaluated);
        }
    }
}
