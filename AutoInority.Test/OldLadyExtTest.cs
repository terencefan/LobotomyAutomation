using AutoInority.Creature;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 老妇人调度优先级：IsUrgent = 计数 ≤2（上限 4）。她唯一回充手段是被工作
    /// （GOOD/NORM +1），观察者机制随全设施工作频率扣点，低位必出逃；
    /// 消费点 Automaton.Main 的 FilterUrgent 优先派工（ CreatureModelExtensions.IsUrgent
    /// → GetExtension().IsUrgent，无 isOverloaded 短路项）。锚点：_qliphothCounter
    /// （私有，反射注；qliphothCounter 为 get-only 属性）。
    /// </summary>
    public class OldLadyExtTest
    {
        private static OldLadyExt ExtWith(int counter)
        {
            var creature = GameBridge.Bare<CreatureModel>();
            GameBridge.Set(creature, "script", GameBridge.Bare<OldLady>());
            GameBridge.Set(creature, "_qliphothCounter", counter);
            return new OldLadyExt(creature);
        }

        [Theory]
        [InlineData(0, true)]
        [InlineData(2, true)]
        [InlineData(3, false)]
        [InlineData(4, false)]
        public void IsUrgentFollowsLowCounter(int counter, bool expected)
        {
            Assert.Equal(expected, ExtWith(counter).IsUrgent);
        }
    }
}
