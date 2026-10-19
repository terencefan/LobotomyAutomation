using System.Collections.Generic;

using AutoInority.Extentions;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 训练门语义（HasTrainableTypes）：聚合版返回「未满级类型集合非空」，
    /// types 只收还没满的四维。历史 bug 锚点——原聚合版误用 HasReachedExpLimit
    /// 命名，TryTrain 守卫随之反转（跳过的恰是有可练类型的员工），训练从不派工。
    /// 链路依赖均离线安全：LocalizeTextDataModel/MissionManager/ResearchDataModel
    /// 均惰性自建空集合，GetText 未知键返回 "UNKNOWN"，MaxStat 上限 130。
    /// </summary>
    [Collection("GameBridge")]
    public class TrainableTypesTest
    {
        // MissionManager 私有构造不初始化 clearedMissions/closedMissions（在-day 初始化），
        // MaxStatR/W/B/P 全链读它——注入 bare 单例 + 空任务列表
        static TrainableTypesTest()
        {
            var missionManager = GameBridge.Bare<MissionManager>();
            typeof(MissionManager)
                .GetField("_instance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .SetValue(null, missionManager);
            GameBridge.Set(missionManager, "clearedMissions", new List<Mission>());
            GameBridge.Set(missionManager, "closedMissions", new List<Mission>());
        }

        private static AgentModel AgentWith(int primary, int exp)
        {
            var agent = GameBridge.CanWorkAgent();
            var primaryStat = new WorkerPrimaryStat
            {
                hp = primary,
                mental = primary,
                work = primary,
                battle = primary,
            };
            GameBridge.Set(agent, "primaryStat", primaryStat);
            var statExp = new WorkerPrimaryStatExp
            {
                hp = exp,
                mental = exp,
                work = exp,
                battle = exp,
            };
            GameBridge.Set(agent, "primaryStatExp", statExp);
            return agent;
        }

        [Fact]
        public void TrainableAgentYieldsAllTypes()
        {
            // 新员工（10/0）：四维全未满 → 可练且集合全类型
            var agent = AgentWith(primary: 10, exp: 0);

            Assert.True(agent.HasTrainableTypes(out var types));
            Assert.Equal(4, types.Count);
        }

        [Fact]
        public void MaxedAgentHasNothingToTrain()
        {
            // 满级员工（130+0 ≥ MaxStat 任意上限）：四维全满 → 不可练、空集
            var agent = AgentWith(primary: 130, exp: 0);

            Assert.False(agent.HasTrainableTypes(out var types));
            Assert.Empty(types);
        }

        [Fact]
        public void PartiallyMaxedAgentYieldsRemainingTypes()
        {
            // 只满勇气一维（130），其余未满：可练且集合恰好 3 类型
            var agent = GameBridge.CanWorkAgent();
            GameBridge.Set(agent, "primaryStat", new WorkerPrimaryStat
            {
                hp = 130,
                mental = 10,
                work = 10,
                battle = 10,
            });
            GameBridge.Set(agent, "primaryStatExp", new WorkerPrimaryStatExp());

            Assert.True(agent.HasTrainableTypes(out var types));
            Assert.Equal(3, types.Count);
            Assert.DoesNotContain(RwbpType.R, types); // hp = 勇气(R)已满，不再练
        }
    }
}
