namespace AutoInority
{
    /// <summary>
    /// 全局可调参数：与 Automaton 单例分离——不随 Reset（结算/重开）回默认，
    /// 二期可由 Angela 自主调节（直接写属性即生效，全模组同 tick 读到新值）。
    /// </summary>
    internal static class AutomationConfig
    {
        /// <summary>
        /// 掉计数器工作的派工及格线：逆卡巴拉计数器 &gt; 1（本次工作会减少计数器、
        /// 有出逃风险）时，普通期望置信度高于此值才放行（ExpectNormalExt /
        /// ExpectGoodAndNormalExt）。调低 = 更敢赌计数器，调高 = 更保守。
        /// </summary>
        public static float CounterDecreaseConfidence { get; set; } = 0.95f;

        /// <summary>
        /// 出逃风险工作的派工及格线：结果可能较差时，工作置信度（不出事的把握）
        /// 高于此值才放行（ExpectNormalExt / MurdererExt / QueenBeeExt 等共享
        /// 旋钮——调它动全模组派工口径）。调高 = 谁都别碰会出逃的异想体。
        /// </summary>
        public static float CreatureEscapeConfidence { get; set; } = 0.99f;

        /// <summary>
        /// 带伤工作的存活及格线：CheckSurvive 按当前 hp/mental 卷积的存活把握
        /// 高于此值才派（BaseCreatureExt.CheckSurvive；WhiteNight/NothingThere
        /// 等怕死人事件的条目同用）；个别异想体反向取 1 - 此值作诱惑边界
        /// （Porccubus：自律不足且良期望越过该线即拒绝）。调高 = 更惜命。
        /// </summary>
        public static float DeadConfidence { get; set; } = 0.99f;

        /// <summary>
        /// 镇压资格的武器效能阈值：异想体防御行对武器伤害类型的乘数低于此值即打不动、
        /// 不派镇压（默认 0.5 = RESISTANCE/IMMUNE 边界，对齐 DefenseInfo.GetDefenseType，
        /// 消费点 IsCapableOfPressing）。调低 = 抗性武器也硬上，调到 0 = 关闭效能闸。
        /// </summary>
        public static float PressingEfficacyThreshold { get; set; } = 0.5f;
    }
}
