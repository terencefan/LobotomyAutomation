using System.Collections.Generic;

using AutoInority;
using AutoInority.Extentions;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 派工省着用排序（Candidate.SufficientComparer）：过门候选按「属性等级升序 →
    /// 置信度降序 → 距离升序」选人。属性等级是成功率查表的真实跳变维度
    /// （AgentModel.CalculateStatLevel：<30=1、30-44=2、45-64=3、65-84=4、≥85=5），
    /// 低价值工作不该消耗 5 级员工。宏员工（Automaton.WorkingAgents 绑定）硬排除出
    /// 管理/farm 候选池。Candidate 用 internal 测试构造器注距离与置信度，
    /// 绕开 Graph.Distance/GetConfidence 的游戏链。
    /// </summary>
    public class CandidateSufficiencyTest
    {
        private static AgentModel StatAgent(int fortitude, int prudence, int temperance, int justice)
        {
            var agent = GameBridge.CanWorkAgent("小红");
            var stat = GameBridge.Bare<WorkerPrimaryStat>();
            // WorkerPrimaryStat 字段映射：hp↔勇气(R)/mental↔谨慎(W)/work↔自律(B)/battle↔正义(P)
            GameBridge.Set(stat, "hp", fortitude);
            GameBridge.Set(stat, "mental", prudence);
            GameBridge.Set(stat, "work", temperance);
            GameBridge.Set(stat, "battle", justice);
            GameBridge.Set(agent, "primaryStat", stat);
            GameBridge.Set(agent, "_statBufList", new List<UnitStatBuf>());
            return agent;
        }

        // id → rwbpType 派生：0=N/1=R/2=W/3=B/4=P（先例 CheckSurviveEquipmentTest）
        private static SkillTypeInfo Skill(int id) => new SkillTypeInfo { id = id };

        private static Candidate Make(AgentModel agent, SkillTypeInfo skill, float distance, double manage = 0.5, double farm = 0.5)
        {
            return new Candidate(agent, GameBridge.Bare<CreatureModel>(), skill, distance, manage, farm);
        }

        [Fact]
        public void StatLevelOfMapsWorkTypes()
        {
            var agent = StatAgent(fortitude: 66, prudence: 29, temperance: 85, justice: 45);
            Assert.Equal(4, Candidate.StatLevelOf(agent, RwbpType.R)); // 66 → 4 级
            Assert.Equal(1, Candidate.StatLevelOf(agent, RwbpType.W)); // 29 → 1 级
            Assert.Equal(5, Candidate.StatLevelOf(agent, RwbpType.B)); // 85 → 5 级
            Assert.Equal(3, Candidate.StatLevelOf(agent, RwbpType.P)); // 45 → 3 级
            Assert.Equal(1, Candidate.StatLevelOf(agent, RwbpType.N)); // 无属性工作恒 1 级
        }

        [Fact]
        public void SufficientComparerPrefersLowerStatLevel()
        {
            // 5 级员工置信度更高也不先用：省着用，低等级（4 级）过门即上
            var level5 = Make(StatAgent(85, 85, 85, 85), Skill(1), 1f, manage: 0.99);
            var level4 = Make(StatAgent(66, 66, 66, 66), Skill(1), 100f, manage: 0.90);

            Assert.True(Candidate.SufficientComparer(level4, level5) < 0);
        }

        [Fact]
        public void SufficientComparerTieBreaksByConfidence()
        {
            var stable = Make(StatAgent(66, 66, 66, 66), Skill(1), 10f, manage: 0.99);
            var shaky = Make(StatAgent(66, 66, 66, 66), Skill(1), 1f, manage: 0.90);

            Assert.True(Candidate.SufficientComparer(stable, shaky) < 0);
        }

        [Fact]
        public void SufficientComparerTieBreaksByDistance()
        {
            var near = Make(StatAgent(66, 66, 66, 66), Skill(1), 1f);
            var far = Make(StatAgent(66, 66, 66, 66), Skill(1), 100f);

            Assert.True(Candidate.SufficientComparer(near, far) < 0);
        }

        [Fact]
        public void ExploitSufficientComparerTieUsesExploitConfidence()
        {
            // 同级：礼物效率（ExploitConfidence）优先于距离
            var efficient = Make(StatAgent(85, 85, 85, 85), Skill(1), 10f, farm: 0.9);
            var wasteful = Make(StatAgent(85, 85, 85, 85), Skill(1), 1f, farm: 0.5);

            Assert.True(Candidate.ExploitSufficientComparer(efficient, wasteful) < 0);
        }

        [Fact]
        public void SameAgentSortsBySkillStatLevel()
        {
            // 同一员工对不同工作类型的画像不同：勇气 85（5 级）的 R 工排在
            // 自律 66（4 级）的 B 工后面——让弱属性去接对应工作
            var agent = StatAgent(fortitude: 85, prudence: 66, temperance: 66, justice: 85);
            var rSkill = Make(agent, Skill(1), 1f);
            var bSkill = Make(agent, Skill(3), 1f);

            Assert.Equal(5, rSkill.StatLevel);
            Assert.Equal(4, bSkill.StatLevel);
            Assert.True(Candidate.SufficientComparer(bSkill, rSkill) < 0);
        }

        [Fact]
        public void IsMacroBoundTracksWorkingAgents()
        {
            try
            {
                var macro = GameBridge.CanWorkAgent("宏哥");
                Automaton.Instance.WorkingAgents[macro] = null;
                Assert.True(macro.IsMacroBound());

                var bystander = GameBridge.CanWorkAgent("路人");
                Assert.False(bystander.IsMacroBound());
            }
            finally
            {
                Automaton.Reset(); // Automaton.Instance 是全局单例，用完拆掉防串测
            }
        }
    }
}
