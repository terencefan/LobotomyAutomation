using System.Collections.Generic;

using AutoInority.Creature;
using AutoInority.Extentions;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 魔弹射手（O-01-45 Der Freischütz，Freischutz.cs）：NORM/BAD 结果都扣计数
    /// （OnReleaseWork L54-73，只有 GOOD 安全），justice&lt;3 完工再扣
    /// （_CONDITION_JUSTICE_LEVEL=3，L69-74），归零=随机部门 B80 魔弹后自动复原。
    /// ①justice 闸 ≤3 → &lt;3：justice=3 做压迫 workProb 0.6 全场最优且不触发完工
    /// 追加扣减，原被误禁；②CheckWorkConfidence 改只接受 GOOD 置信——基类
    /// counter&gt;1 的普通置信缓冲会给 NORM 留磨计数口子，一律关闭；基类 Exploit 放宽臂
    /// （IsExploiting &amp;&amp; AutoSuppress）只对 TETH 以下生效，HE 本就不可达；过载短路
    /// 保留（进房即 CancelOverload，拒绝派工反而等 60s 过载引信走完强制归零，
    /// 短路严格更安全）。
    /// 置信经 ScriptedFreischutzExt 注入工作成功率（先例 ShyLookDoorTest.ProbAtDoor），
    /// 界值取游戏真数据 Freischutz_stat.txt（NormalBound 6 / GoodBound 11 / MaxCube 18）：
    /// GOOD=P(≥12)、普通=P(≥7)；生产成功率钳位 0.95（CalculateWorkSuccessProb），
    /// GOOD 线 p*≈0.855、普通线 p*≈0.554。p=0.7 时普通置信 0.999 过 0.95 线而
    /// GOOD 0.722 不过 0.99 线——新旧判别窗口，删 override 本类即红。
    /// </summary>
    [Collection("GameBridge")] // GetExtension 静态缓存 + SkillTypeList 单例，跨类串行
    public class DerFreischutzExtTest
    {
        private sealed class ScriptedFreischutzExt : DerFreischutzExt
        {
            private readonly float _prob;

            internal ScriptedFreischutzExt(CreatureModel creature, float prob) : base(creature)
            {
                _prob = prob;
            }

            protected override float CalculateWorkSuccessProb(AgentModel agent, SkillTypeInfo skill) => _prob;
        }

        /// <summary>界值/名称/riskLevel 注游戏真数据（Freischutz_stat.txt cube 6/11/18）。</summary>
        private static CreatureModel Creature(int counter, bool overloaded = false)
        {
            return GameBridge.Creature<Freischutz>("魔弹射手", "HE", new[] { 6, 11, 18 }, counter, overloaded);
        }

        private static AgentModel Agent(int justiceStat)
        {
            var agent = GameBridge.CanWorkAgent("小红");
            var stat = GameBridge.Bare<WorkerPrimaryStat>();
            GameBridge.Set(stat, "battle", justiceStat);
            GameBridge.Set(agent, "primaryStat", stat);
            // justiceLevel 链：GetPrimaryStatBuf 遍历 _statBufList（独立于 _bufList）
            GameBridge.Set(agent, "_statBufList", new List<UnitStatBuf>());
            return agent;
        }

        private static SkillTypeInfo Skill()
        {
            return new SkillTypeInfo { id = 4, name = "P", calledName = "压迫" };
        }

        [Fact]
        public void FactoryResolvesHeBranch()
        {
            GameBridge.InjectSkillTypes();
            Assert.IsType<DerFreischutzExt>(Creature(3).GetExtension());
        }

        [Fact]
        public void CanWorkWithPassesJustice3()
        {
            // justice 3 级（battle=50）：完工不追加扣减，压迫 0.6 全场最优，不应拒
            GameBridge.InjectSkillTypes();
            var ext = new DerFreischutzExt(Creature(3));
            Assert.True(ext.CanWorkWith(Agent(50), Skill(), out var message));
            Assert.Null(message);
        }

        [Fact]
        public void CanWorkWithRejectsJustice2()
        {
            GameBridge.InjectSkillTypes();
            var ext = new DerFreischutzExt(Creature(3));
            Assert.False(ext.CanWorkWith(Agent(30), Skill(), out var message));
            Assert.NotNull(message);
            Assert.Contains("小红", message);
            Assert.Contains("魔弹射手", message);
            Assert.Contains("正义", message);
        }

        [Fact]
        public void CheckWorkConfidenceRequiresGoodEvenWithCounterBuffer()
        {
            // counter=3 时基类走普通置信缓冲：p=0.7 的 P(≥7)=0.999>0.95 会接受；
            // GOOD-only 下 P(≥12)=0.722 不过 0.99 线，必须弹回——删 override 本测试即红
            GameBridge.InjectSkillTypes();
            var ext = new ScriptedFreischutzExt(Creature(3), 0.7f);
            Assert.False(ext.CheckWorkConfidence(Agent(50), Skill()));
        }

        [Fact]
        public void CheckWorkConfidenceAcceptsClampedMaxProb()
        {
            // 生产成功率钳位 0.95（CalculateWorkSuccessProb，生产不可超）：
            // P(≥12)≈0.9999 严格大于及格线 0.99，counter=3 也可派
            GameBridge.InjectSkillTypes();
            var ext = new ScriptedFreischutzExt(Creature(3), 0.95f);
            Assert.True(ext.CheckWorkConfidence(Agent(50), Skill()));
        }

        [Fact]
        public void CheckWorkConfidenceOverloadedShortCircuit()
        {
            // 过载短路在 counter 2-3 也生效（基类仅在 counter≤1 短路）：
            // 进房即 CancelOverload，一次 NORM 只扣一格；拒绝派工则 60s 过载引信
            // 走完 ExplodeOverload 强制归零，短路是严格更安全的选择
            GameBridge.InjectSkillTypes();
            var ext = new ScriptedFreischutzExt(Creature(3, overloaded: true), 0.01f);
            Assert.True(ext.CheckWorkConfidence(Agent(50), Skill()));
        }
    }
}
