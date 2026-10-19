namespace AutoInority.Posture
{
    /// <summary>
    /// 调度姿态：生产（开发 + 日常派工）、训练（经验缺口驱动，弱者优先）、
    /// 镇压（生产休眠，专注镇压与紧急应对）。B 键按枚举声明序循环。
    /// 应急反射不属于任何姿态——三姿态公共基座（Automaton.Main 统一扫描）。
    /// </summary>
    internal enum PostureEnum
    {
        PRODUCTION,

        TRAINING,

        SUPPRESS,
    }
}
