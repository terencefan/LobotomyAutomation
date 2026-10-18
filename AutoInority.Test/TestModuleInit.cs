using System.Runtime.CompilerServices;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 测试程序集加载即注入 N/R/W/B/P 五型技能表：BaseCreatureExt 的静态技能字段
    /// 在 cctor 一次性捕获，任何测试类先于 InjectSkillTypes 触碰其子类都会把 null
    /// 钉死进字段——xunit 类调度顺序不保证，该竞态在类集合变化时会随机引爆
    /// （GameBridgeSmokeTest/LadyLookingAtWallTest 的 SkillSets 断言曾因此 CI 红、
    /// 本地绿）。ModuleInitializer 先于一切测试类运行，从根上消灭时序前提。
    /// </summary>
    internal static class TestModuleInit
    {
        [ModuleInitializer]
        internal static void Init()
        {
            GameBridge.InjectSkillTypes();
        }
    }
}
