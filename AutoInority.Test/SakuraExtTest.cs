using AutoInority.Creature;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 樱下墓（O-04-100）献祭救援：ResolveAttract 按「连点 5 次唤醒」
    /// （Uncontrollable_Sakura._neededClicked=5，WakeUp 后 2s 原版释放）一个 tick
    /// 打满阈值；其他吸引类型保持单点节奏不被误点。
    /// </summary>
    [Collection("GameBridge")] // GameBridge 静态反射，跨类串行
    public class SakuraExtTest
    {
        private sealed class CountingSakura : Uncontrollable_Sakura
        {
            public int Clicks;

            public CountingSakura(WorkerModel worker) : base(worker, GameBridge.Bare<Sakura>())
            {
            }

            public override void OnClick() => Clicks++;
        }

        private sealed class CountingOther : UncontrollableAction
        {
            public int Clicks;

            public override void OnClick() => Clicks++;
        }

        private static WorkerModel WorkerWith(UncontrollableAction action)
        {
            var worker = GameBridge.Bare<WorkerModel>();
            GameBridge.Set(worker, "unconAction", action);
            return worker;
        }

        [Fact]
        public void ResolveAttractClicksWakeUpThreshold()
        {
            // 5 = _neededClicked 唤醒阈值；多余点击由产品 OnClick 的 wakeUp 守卫吸收
            var sakura = new CountingSakura(GameBridge.Bare<WorkerModel>());
            SakuraExt.ResolveAttract(WorkerWith(sakura));
            Assert.Equal(5, sakura.Clicks);
        }

        [Fact]
        public void ResolveAttractIgnoresOtherActions()
        {
            var other = new CountingOther();
            SakuraExt.ResolveAttract(WorkerWith(other));
            Assert.Equal(0, other.Clicks);
            // 无吸引动作的裸员工不炸
            SakuraExt.ResolveAttract(WorkerWith(null));
        }

        [Fact]
        public void CounterDoesNotGateWorkConfidence()
        {
            // 工厂兜底 ExpectGoodAndNormalExt 在 counter<=1 转 GOOD-only 保守闸；
            // 樱下墓直连 BaseCreatureExt：任何计数下置信检查恒过（生存闸另计）
            GameBridge.InjectSkillTypes();
            var creature = GameBridge.Bare<CreatureModel>();
            GameBridge.Set(creature, "script", GameBridge.Bare<Sakura>());
            GameBridge.Set(creature, "_qliphothCounter", 1);
            var ext = new SakuraExt(creature);
            Assert.False(ext is ExpectGoodAndNormalExt);
            Assert.True(ext.CheckWorkConfidence(GameBridge.CanWorkAgent("小红"), new SkillTypeInfo()));
        }
    }
}
