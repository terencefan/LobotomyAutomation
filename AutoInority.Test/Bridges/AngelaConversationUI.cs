/// <summary>
/// AngelaConversationUI 测试替身：与真游戏类型同名（全局命名空间），源码类型遮蔽
/// 引用程序集（CS0436，源码优先，csproj NoWarn 压制）。真身 AddAngelaMessage 体内
/// 引用 UnityEngine.UI icall（Behaviour.set_enabled 等），CoreCLR JIT 编译整个方法体
/// 时即解析 icall 并抛 SecurityException(ECall)——守卫短路（_isLock/gameMode=TUTORIAL）
/// 均于事无补，离线测试进程物理无法调用该真身（无 icall 引用的 Lock() 可正常通过）。
/// 替身只保留 mod 绑定面（Angela.Say → instance.AddAngelaMessage），语义为气泡锁死
/// no-op（等同游戏 _isLock=true）；Announce 的 Log 半边照走 Notice 管道可被观测。
/// 真 API 漂移由产品 mcs 编译守门，此处不重复校验。
/// </summary>
public class AngelaConversationUI
{
    internal static AngelaConversationUI instance { get; set; }

    public void AddAngelaMessage(string desc) { }
}
