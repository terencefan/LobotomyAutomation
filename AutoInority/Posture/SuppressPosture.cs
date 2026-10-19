namespace AutoInority.Posture
{
    /// <summary>
    /// 镇压姿态：生产休眠（不 seed 宏/种地），专注镇压与紧急应对。
    /// </summary>
    internal class SuppressPosture : PostureBase
    {
        public override void OnTick()
        {
            EnqueueThreats();
        }
    }
}
