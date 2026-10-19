namespace AutoInority.Creature
{
    using System;

    /// <summary>
    /// buf 危害排除的纯求值器（与游戏类型零耦合，供真值表测试固化）：
    /// 表行为 = 首个「命中且未豁免」的规则决定拒绝，全不命中（或空表）放行。
    /// 规则行持已捕获上下文的闭包，游戏类型绑定与文案格式化（命中才 Tag()，
    /// 与手写版拒绝时才取名的时机一致）都留给调用方。
    /// </summary>
    internal static class BufExclusionRule
    {
        internal sealed class Rule
        {
            internal readonly Func<bool> Match;

            internal readonly Func<bool> Exempt;

            internal readonly string Message;

            internal Rule(Func<bool> match, Func<bool> exempt, string message)
            {
                Match = match;
                Exempt = exempt;
                Message = message;
            }
        }

        /// <summary>遍历规则表，返回首个「命中且未豁免」的规则；null = 放行。
        /// 文案模板原样透传，按位格式化（{0}员工 {1}收容物 {2}技能）由调用方完成。</summary>
        internal static Rule Evaluate(Rule[] rules)
        {
            foreach (var rule in rules)
            {
                if (rule.Match() && !rule.Exempt())
                {
                    return rule;
                }
            }
            return null;
        }
    }
}
