namespace AutoInority.Posture
{
    /// <summary>
    /// 调度姿态：生产（宏/种地 + 应急反射）与镇压（应急反射 + 撤离分级，生产休眠）。
    /// </summary>
    internal enum PostureEnum
    {
        PRODUCTION,

        SUPPRESS,
    }
}
