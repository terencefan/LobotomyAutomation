using System.Collections.Generic;

using AutoInority.Extentions;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// GetBarrierValue 的护盾剩余量读取：遍历 UnitBuf 列表，累计同类型与通用（A）护盾的
    /// _barrierValue，无护盾或类型不匹配返回 0；只反射读字段，不触碰有副作用的 UseBarrier。
    /// 行为锚点：CheckSurvive 把对应类型的护盾剩余抵扣量折算进有效血池。
    /// </summary>
    public class BarrierValueTest
    {
        private static AgentModel AgentWithBufs(params UnitBuf[] bufs)
        {
            var agent = GameBridge.Bare<AgentModel>();
            GameBridge.Set(agent, "_bufList", new List<UnitBuf>(bufs));
            return agent;
        }

        [Fact]
        public void NoBufsYieldsZero()
        {
            Assert.Equal(0f, AgentWithBufs().GetBarrierValue(RwbpType.B));
        }

        [Fact]
        public void TypedAndUniversalBarriersSum()
        {
            var agent = AgentWithBufs(
                new BarrierBuf(RwbpType.B, 30f, 60f),
                new BarrierBuf(RwbpType.A, 20f, 60f),
                new UnitBuf());
            Assert.Equal(50f, agent.GetBarrierValue(RwbpType.B));
            Assert.Equal(20f, agent.GetBarrierValue(RwbpType.A));
            // A 型通用护盾对任意伤害类型都抵扣，W 查询同样只吃到它。
            Assert.Equal(20f, agent.GetBarrierValue(RwbpType.W));
        }

        [Fact]
        public void DepletedBarrierReadsZero()
        {
            var agent = AgentWithBufs(new BarrierBuf(RwbpType.R, 0f, 60f));
            Assert.Equal(0f, agent.GetBarrierValue(RwbpType.R));
        }
    }
}
