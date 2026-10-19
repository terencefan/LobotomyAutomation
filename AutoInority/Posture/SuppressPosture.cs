namespace AutoInority.Posture
{
    /// <summary>
    /// 镇压姿态：生产休眠（不 seed 开发/训练），专注镇压与紧急应对。
    /// 清剿命令由三姿态公共基座（Main 的应急反射）承担，本姿态 OnTick 为空——
    /// 语义即「停掉一切日常，只留基座」；二期分级撤离在此扩展。
    /// </summary>
    internal class SuppressPosture : PostureBase
    {
        public override void OnTick()
        {
        }
    }
}
