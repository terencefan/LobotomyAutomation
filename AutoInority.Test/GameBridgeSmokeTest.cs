using System.Collections.Generic;
using System.Linq;

using AutoInority.Creature;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 桥接层冒烟：验证「挂载真游戏 DLL + 跳构造器分配 + 注字段」的供给链，
    /// 以及注值后游戏真实现（UnitModel buf 方法/Notice 管道/Automaton 置信度）可离线驱动。
    /// 反射字段名为游戏反编译锚点，重命名时在此显式失败。
    /// </summary>
    [Collection("GameBridge")] // 共享 SkillTypeList/Notice/Automaton 全局单例，跨类串行
    public class GameBridgeSmokeTest
    {
        [Fact]
        public void BareAllocationResolvesInheritedFields()
        {
            var agent = GameBridge.Bare<AgentModel>();
            var bufs = new List<UnitBuf>();
            GameBridge.Set(agent, "_bufList", bufs);
            Assert.Same(bufs, GameBridge.Get(agent, "_bufList"));
        }

        [Fact]
        public void UnitModelBufMethodsRunRealImplementation()
        {
            var agent = GameBridge.Bare<AgentModel>();
            GameBridge.Set(agent, "_bufList", new List<UnitBuf>());

            // 注值前无 buf（游戏真实现遍历 _bufList）
            Assert.False(agent.HasUnitBuf(UnitBufType.LITTLEWITCH_HEART));

            var heart = GameBridge.Bare<UnitBuf>();
            heart.type = UnitBufType.LITTLEWITCH_HEART;
            var fairy = GameBridge.Bare<FairyBuf>();
            agent.GetUnitBufList().Add(heart);
            agent.GetUnitBufList().Add(fairy);

            // HasUnitBuf/GetUnitBufList 走 Assembly-CSharp 内的实机代码，非测试抄写
            Assert.True(agent.HasUnitBuf(UnitBufType.LITTLEWITCH_HEART));
            Assert.Contains(agent.GetUnitBufList(), x => x is FairyBuf);
            Assert.Equal(2, agent.GetUnitBufList().Count);
        }

        [Theory]
        [InlineData(0, RwbpType.N)]
        [InlineData(1, RwbpType.R)]
        [InlineData(2, RwbpType.W)]
        [InlineData(3, RwbpType.B)]
        [InlineData(4, RwbpType.P)]
        public void InjectedSkillTypesFeedRealLookup(int id, RwbpType expected)
        {
            GameBridge.InjectSkillTypes();
            // name 是测试注入值，rwbpType 是游戏按 id 派生的真 property——一并钉住
            Assert.Equal(expected, SkillTypeList.instance.GetData(id).rwbpType);
        }

        [Fact]
        public void BaseCreatureExtStaticSkillsResolveAfterInjection()
        {
            // 静态字段（Attachment/Insight/Instinct/Repression）在类型初始化时一次性捕获；
            // 若任何用例先于 InjectSkillTypes 触碰派生类，字段静默为 null——此用例钉住时序契约
            GameBridge.InjectSkillTypes();
            var creature = GameBridge.Bare<CreatureModel>();
            GameBridge.Set(creature, "script", GameBridge.Bare<ShyThing>());
            var sets = new ShyLook(creature).SkillSets;
            Assert.Equal(4, sets.Length);
            Assert.All(sets, Assert.NotNull);
        }

        [Fact]
        public void NoticePipeCapturesSystemLog()
        {
            var captured = new List<object[]>();
            Notice.instance.Observe("AddSystemLog", param => captured.Add(param));
            Notice.instance.Send("AddSystemLog", "hello");
            Assert.Single(captured);
            Assert.Equal("hello", captured[0][0]);
        }

        [Fact]
        public void AutomatonConfidenceDefaultsReadable()
        {
            try
            {
                Assert.Equal(0.99f, Automaton.Instance.DeadConfidence);
                Automaton.Instance.DeadConfidence = 0.5f;
                Assert.Equal(0.5f, Automaton.Instance.DeadConfidence);
            }
            finally
            {
                // 断言失败也不能把改过的置信度漏给后续用例
                Automaton.Reset();
            }
        }

        private sealed class PlainExt : BaseCreatureExt
        {
            internal PlainExt(CreatureModel creature) : base(creature)
            {
            }
        }

        [Fact]
        public void CanWorkWithChainRunsRealImplementation()
        {
            // CanWorkWith 全链（Laetitia 心 → 精灵祝福 → 碎甲装备 → FriendToken）
            // 在桥接空表下走游戏真实现直至放行；链上新挂检查点若依赖未注字段在此显式 NRE
            GameBridge.InjectSkillTypes();
            var look = new PlainExt(GameBridge.Bare<CreatureModel>()); // Script 留 null：非 LittleWitch/GalaxyBoy 特判照走
            Assert.True(look.CanWorkWith(GameBridge.CanWorkAgent(), new SkillTypeInfo { id = 3, name = "B", calledName = "本能" }, out var message));
            Assert.Null(message);
        }
    }
}
