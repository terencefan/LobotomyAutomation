using System.Collections.Generic;
using System.Reflection;

using AutoInority.Creature;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 你必须要幸福：使用次数护栏 + 按四维缺口定制的最优停点（YouMustHappy.cs）。
    /// 护栏：同一员工第 6 次使用即失控死亡（OnReleaseWork usedCount>=6 → LoseControl+
    /// Dead 动画），自动化放行到 5 次（计数每日清零且不分派发者）。停点：buff 量 =
    /// elapsed²/6（凸性），CanStop 须「elapsed ≥ √(6·v*) 且符号为 YES」，v* = 使
    /// Σ min(v+3, gap) ≤ 199 的最大 v（≥200 进日结死亡名单；+3 覆盖 YES 等待期增长）。
    /// 另设硬顶：elapsed 越过 √(6·(v_hard−1)) 哨位即无条件停（不看符号）——卡顿等
    /// 极端下落 NO 吃小负 buf 也好过 YES 结算 ≥200；缺口总和 ≤199 者永不过载、无硬顶。
    /// gap = 100 − (有效stat − 旧buf值) 镜像结算时点（OnReleaseWork 先剥旧 buf 再算
    /// clamp）。buff 替换制（ONLY_ONE）下旧值 ≥ 新掷保底则拒派省次数，负 buff 放行救援。
    /// </summary>
    [Collection("GameBridge")]
    public class YouMustBeHappyExtTest
    {
        private static YouMustHappy ScriptWithUsage(AgentModel agent, int usedCount)
        {
            var script = GameBridge.Bare<YouMustHappy>();
            if (agent != null)
            {
                var nested = typeof(YouMustHappy).GetNestedType("UsedAgent", BindingFlags.NonPublic);
                var entry = System.Activator.CreateInstance(nested, agent);
                entry.GetType().GetField("usedCount").SetValue(entry, usedCount);
                var listType = typeof(YouMustHappy)
                    .GetField("_usedAgentList", BindingFlags.NonPublic | BindingFlags.Instance).FieldType;
                var list = System.Activator.CreateInstance(listType);
                listType.GetMethod("Add").Invoke(list, new[] { entry });
                GameBridge.Set(script, "_usedAgentList", list);
            }
            return script;
        }

        /// <summary>CanWorkAgent 不带属性；四维注入需 primaryStat + _statBufList 双注
        /// （fortitudeStat 聚合遍历 _statBufList，先例 LadyLookingAtWallTest）。</summary>
        private static AgentModel StatAgent(int f, int p, int j, int t)
        {
            var agent = GameBridge.CanWorkAgent();
            var stat = GameBridge.Bare<WorkerPrimaryStat>();
            GameBridge.Set(stat, "hp", f);
            GameBridge.Set(stat, "mental", p);
            GameBridge.Set(stat, "battle", j);
            GameBridge.Set(stat, "work", t);
            GameBridge.Set(agent, "primaryStat", stat);
            GameBridge.Set(agent, "_statBufList", new List<UnitStatBuf>());
            return agent;
        }

        /// <summary>幸福 buf 双列表同注：_bufList 供 GetUnitBufByType、_statBufList 供
        /// 四维聚合（镜像游戏 AddUnitBuf）；primaryStat 构造器已初始化零值，直接赋。</summary>
        private static void WithHappyBuf(AgentModel agent, int hp, int mn, int bt, int wk)
        {
            var buf = new UnitStatBuf(9999f, UnitBufType.YOU_MUST_HAPPY);
            buf.primaryStat.hp = hp;
            buf.primaryStat.mental = mn;
            buf.primaryStat.battle = bt;
            buf.primaryStat.work = wk;
            ((List<UnitBuf>)GameBridge.Get(agent, "_bufList")).Add(buf);
            ((List<UnitStatBuf>)GameBridge.Get(agent, "_statBufList")).Add(buf);
        }

        private static YouMustBeHappyExt Ext(YouMustHappy script, AgentModel worker = null)
        {
            var creature = GameBridge.Bare<CreatureModel>();
            GameBridge.Set(creature, "script", script);
            if (worker != null)
            {
                var skill = GameBridge.Bare<UseSkill>();
                skill.agent = worker;
                GameBridge.Set(creature, "_currentSkill", skill);
            }
            return new YouMustBeHappyExt(creature);
        }

        [Fact]
        public void TargetBuffWhiteboard()
        {
            Assert.Equal(46, YouMustBeHappyExt.TargetBuff(new[] { 90, 90, 90, 90 }));
        }

        [Fact]
        public void TargetBuffHighStat()
        {
            Assert.Equal(5, YouMustBeHappyExt.TargetBuff(new[] { 5, 5, 5, 5 }));
        }

        [Fact]
        public void TargetBuffMixed()
        {
            // 3×min(v+3,90)+min(v+3,10) ≤ 199 → v=60 时 3×63+10=199 贴线
            Assert.Equal(60, YouMustBeHappyExt.TargetBuff(new[] { 90, 90, 90, 10 }));
        }

        [Fact]
        public void TargetBuffSingle()
        {
            Assert.Equal(99, YouMustBeHappyExt.TargetBuff(new[] { 99, 0, 0, 0 }));
        }

        [Fact]
        public void TargetBuffZero()
        {
            // 全满维：无收益可图，CanStop 退化为首-YES 停
            Assert.Equal(0, YouMustBeHappyExt.TargetBuff(new[] { 0, 0, 0, 0 }));
        }

        [Fact]
        public void HardCapWhiteboard()
        {
            // Σ min(v,90)=4v ≤ 199 → v_hard=49
            Assert.Equal(49, YouMustBeHappyExt.HardCapValue(new[] { 90, 90, 90, 90 }));
        }

        [Fact]
        public void HardCapMixed()
        {
            // 3min(v,90)+min(v,10) ≤ 199 → 3×63+10=199 贴线 → v_hard=63
            Assert.Equal(63, YouMustBeHappyExt.HardCapValue(new[] { 90, 90, 90, 10 }));
        }

        [Fact]
        public void HardCapNoneWhenTotalGapSmall()
        {
            // Σgap=20 < 200：无论多久都不过载，无硬顶
            Assert.Equal(int.MaxValue, YouMustBeHappyExt.HardCapValue(new[] { 5, 5, 5, 5 }));
        }

        [Fact]
        public void CanStopHardCapBypassesSign()
        {
            // 白板 v_hard=49，哨位 t=√(6×48)≈16.97s：哨位后 NO 相位也停（吃小负 buf
            // 好过 YES 结算 ≥200 融毁）；哨位前 NO 照旧等翻转
            var agent = StatAgent(10, 10, 10, 10);
            var script = ScriptWithUsage(null, 0);
            var ext = Ext(script, agent);
            GameBridge.Set(script, "_succeedUsing", false);
            GameBridge.Set(script, "_elapsedUsingTime", 16.9f);
            Assert.False(ext.CanStop());
            GameBridge.Set(script, "_elapsedUsingTime", 17f);
            Assert.True(ext.CanStop());
        }

        [Fact]
        public void CanStopRequiresYes()
        {
            var agent = StatAgent(10, 10, 10, 10);
            var script = ScriptWithUsage(null, 0);
            var ext = Ext(script, agent);
            GameBridge.Set(script, "_elapsedUsingTime", 10f);
            GameBridge.Set(script, "_succeedUsing", false);
            Assert.False(ext.CanStop());
            GameBridge.Set(script, "_elapsedUsingTime", 30f);
            GameBridge.Set(script, "_succeedUsing", true);
            Assert.True(ext.CanStop());
        }

        [Fact]
        public void CanStopWaitsForTargetTime()
        {
            // 白板四维 10：v*=46，t*=√276≈16.6s
            var agent = StatAgent(10, 10, 10, 10);
            var script = ScriptWithUsage(null, 0);
            var ext = Ext(script, agent);
            GameBridge.Set(script, "_succeedUsing", true);
            GameBridge.Set(script, "_elapsedUsingTime", 10f);
            Assert.False(ext.CanStop());
            GameBridge.Set(script, "_elapsedUsingTime", 17f);
            Assert.True(ext.CanStop());
        }

        [Fact]
        public void CanStopHighStatEarly()
        {
            // 四维 95：v*=5，t*=√30≈5.5s
            var agent = StatAgent(95, 95, 95, 95);
            var script = ScriptWithUsage(null, 0);
            var ext = Ext(script, agent);
            GameBridge.Set(script, "_succeedUsing", true);
            GameBridge.Set(script, "_elapsedUsingTime", 5f);
            Assert.False(ext.CanStop());
            GameBridge.Set(script, "_elapsedUsingTime", 6f);
            Assert.True(ext.CanStop());
        }

        [Fact]
        public void CanStopMixedStats()
        {
            // temperance 90 → gap 10；v*=60，t*=√360≈19.0s
            var agent = StatAgent(10, 10, 10, 90);
            var script = ScriptWithUsage(null, 0);
            var ext = Ext(script, agent);
            GameBridge.Set(script, "_succeedUsing", true);
            GameBridge.Set(script, "_elapsedUsingTime", 18f);
            Assert.False(ext.CanStop());
            GameBridge.Set(script, "_elapsedUsingTime", 19f);
            Assert.True(ext.CanStop());
        }

        [Fact]
        public void CanStopMirrorsExistingBuf()
        {
            // 结算先剥旧 buf 再 clamp：+50 勇气 buf 的白板员工镜像回 10 → gap 90×4
            // → v*=46，t*≈16.6s（与无 buf 时相同）。漏镜像会把有效值 60 当基础值
            // 算出 gap 40/维 → v*=50、t*≈17.3s——16.5/17 两点区分两种实现
            var agent = StatAgent(10, 10, 10, 10);
            WithHappyBuf(agent, 50, 0, 0, 0);
            var script = ScriptWithUsage(null, 0);
            var ext = Ext(script, agent);
            GameBridge.Set(script, "_succeedUsing", true);
            GameBridge.Set(script, "_elapsedUsingTime", 16.5f);
            Assert.False(ext.CanStop());
            GameBridge.Set(script, "_elapsedUsingTime", 17f);
            Assert.True(ext.CanStop());
        }

        [Fact]
        public void CanStopMaxStatImmediate()
        {
            var agent = StatAgent(100, 100, 100, 100);
            var script = ScriptWithUsage(null, 0);
            var ext = Ext(script, agent);
            GameBridge.Set(script, "_succeedUsing", true);
            GameBridge.Set(script, "_elapsedUsingTime", 0f);
            Assert.True(ext.CanStop());
        }

        [Fact]
        public void CanUseAcceptsFreshAgent()
        {
            // Bare 无构造器：_usedAgentList 字段为 null → 视为 0 次
            Assert.True(Ext(ScriptWithUsage(GameBridge.CanWorkAgent(), 0)).CanUse(GameBridge.CanWorkAgent()));
        }

        [Fact]
        public void CanUseAcceptsThirdUse()
        {
            var agent = GameBridge.CanWorkAgent();
            Assert.True(Ext(ScriptWithUsage(agent, 3)).CanUse(agent));
        }

        [Fact]
        public void CanUseAcceptsFourthUse()
        {
            var agent = GameBridge.CanWorkAgent();
            Assert.True(Ext(ScriptWithUsage(agent, 4)).CanUse(agent));
        }

        [Fact]
        public void CanUseRejectsFifthUse()
        {
            var agent = GameBridge.CanWorkAgent();
            Assert.False(Ext(ScriptWithUsage(agent, 5)).CanUse(agent));
        }

        [Fact]
        public void CanUseAllowsUpgrade()
        {
            // 旧 +20×4（和 80）＜新掷保底 184 → 放行升级重掷
            var agent = StatAgent(10, 10, 10, 10);
            WithHappyBuf(agent, 20, 20, 20, 20);
            Assert.True(Ext(ScriptWithUsage(null, 0), agent).CanUse(agent));
        }

        [Fact]
        public void CanUseRejectsRerollAtTarget()
        {
            // 旧 +46×4（和 184）≥ 新掷保底（镜像 gap 90×4 → v*=46 → 保底 184）→ 省次数
            var agent = StatAgent(10, 10, 10, 10);
            WithHappyBuf(agent, 46, 46, 46, 46);
            Assert.False(Ext(ScriptWithUsage(null, 0), agent).CanUse(agent));
        }

        [Fact]
        public void CanUseRescuesNegativeBuf()
        {
            // 负 buff（暂停/手动 NO 相位结算）：无条件放行救援重掷
            var agent = StatAgent(10, 10, 10, 10);
            WithHappyBuf(agent, -15, -15, -15, -15);
            Assert.True(Ext(ScriptWithUsage(null, 0), agent).CanUse(agent));
        }
    }
}
