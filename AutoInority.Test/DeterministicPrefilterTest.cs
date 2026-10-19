using System;
using System.Collections.Generic;
using System.Reflection;

using AutoInority.Creature;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 确定性预筛（CheckConfidence 第一子句）：CalculateWorkSuccessProb ≤ 0 的
    /// （员工，工作）对必败，应在进入 CheckWorkConfidence/CheckSurvive 的二项卷积
    /// 之前出局。探针 ext 记录 CheckWorkConfidence 是否被查询，以此区分「预筛拦截」
    /// 与「置信度拦截」。查表成功率由 metaInfo.workProbTable 驱动（默认构造即全零表），
    /// UNLIMIT_MODE 短路观察加成，部门死人惩罚（−0.5）可把正基础概率压成 0。
    /// </summary>
    [Collection("GameBridge")] // GlobalGameManager._instance 全局，跨类串行
    public class DeterministicPrefilterTest
    {
        // StubExt 实例化会触发 BaseCreatureExt cctor 一次性捕获静态技能字段，
        // 必须先于该触碰注入技能表（GameBridgeSmokeTest 钉住的时序契约）
        static DeterministicPrefilterTest()
        {
            GameBridge.InjectSkillTypes();
        }

        private sealed class ProbeExt : BaseCreatureExt
        {
            internal bool WorkConfidenceQueried;

            internal ProbeExt(CreatureModel creature) : base(creature)
            {
            }

            public override bool CheckWorkConfidence(AgentModel agent, SkillTypeInfo skill)
            {
                WorkConfidenceQueried = true;
                return true;
            }
        }

        // id=2 → rwbpType 属性派生为 W（1..4 → R/W/B/P），走 prudenceLevel 查表
        private static readonly SkillTypeInfo InsightWork = new SkillTypeInfo { id = 2 };

        [Fact]
        public void ZeroProbShortCircuitsBeforeConfidence()
        {
            using var ggm = UnlimitedMode();
            var ext = ProbeOf(_ => { }); // workProbTable 保持默认全零
            var agent = Agent(prudence: 66); // 4 级也救不了全零表

            Assert.False(ext.CheckConfidence(agent, InsightWork));
            Assert.False(ext.WorkConfidenceQueried); // 预筛先于置信度门
            Assert.False(ext.CheckConfidence(agent, InsightWork));
        }

        [Fact]
        public void PositiveProbReachesConfidence()
        {
            using var ggm = UnlimitedMode();
            var ext = ProbeOf(table => table.SetWorkProb(RwbpType.W, 4, 0.4f));
            var agent = Agent(prudence: 66); // 谨慎 66 → 4 级 → 命中 0.4 档

            ext.CheckConfidence(agent, InsightWork); // 概率门结果不关心，探针必须被触碰

            Assert.True(ext.WorkConfidenceQueried);
        }

        [Fact]
        public void DeadPenaltyCanZeroOutProb()
        {
            using var ggm = UnlimitedMode();
            var ext = ProbeOf(table => table.SetWorkProb(RwbpType.W, 4, 0.3f), deadPenalty: true);
            var agent = Agent(prudence: 66);

            // 0.3 − 0.5 → 负数被 max(0) 钳平 → 确定性出局，不触碰概率门
            Assert.False(ext.CheckConfidence(agent, InsightWork));
            Assert.False(ext.WorkConfidenceQueried);
        }

        private static AgentModel Agent(int prudence)
        {
            var agent = GameBridge.CanWorkAgent("谨慎工");
            // 四维注入需 primaryStat + _statBufList 双注（YouMustBeHappyExtTest 先例）
            GameBridge.Set(agent, "_statBufList", new List<UnitStatBuf>());
            var primaryStat = new WorkerPrimaryStat();
            // WorkerPrimaryStat 字段映射：mental↔谨慎(W)（CandidateSufficiencyTest 先例）
            GameBridge.Set(primaryStat, "mental", prudence);
            // 字段初始化器默认 10：不归零 work 的话 primaryStat.workProb(=work)=10
            // 会给成功率公式垫 0.02 底数，全零表测不出确定性预筛
            GameBridge.Set(primaryStat, "work", 0);
            GameBridge.Set(agent, "primaryStat", primaryStat);
            GameBridge.Set(agent, "additionalDef", new DefenseInfo());
            agent.hp = 100f;
            agent.mental = 100f;
            GameBridge.Set(agent, "baseMaxHp", 100);
            return agent;
        }

        private static ProbeExt ProbeOf(Action<CreatureWorkProbTable> configure, bool deadPenalty = false)
        {
            var creature = GameBridge.Bare<CreatureModel>();
            GameBridge.Set(creature, "script", new CreatureBase());
            GameBridge.Set(creature, "_unit", GameBridge.Bare<CreatureUnit>());
            GameBridge.Set(creature, "_bufList", new List<UnitBuf>());
            var sefira = GameBridge.Bare<Sefira>();
            // agentDeadPenaltyActivated 是私有 _agentDeadPenaltyActivated 的只读属性
            GameBridge.Set(sefira, "_agentDeadPenaltyActivated", deadPenalty);
            GameBridge.Set(creature, "sefira", sefira);

            var metaInfo = GameBridge.Bare<CreatureTypeInfo>();
            // name/riskLevel 属性走 dataTable 管道（未知 key 会触发 LogError 的引擎 icall，必须绕开）
            var nameList = new CreatureTypeInfo.CreatureDataList { itemName = "name" };
            GameBridge.Set(nameList, "list", new List<CreatureTypeInfo.CreatureData>
            {
                new CreatureTypeInfo.CreatureData { data = "测试体", openLevel = 0 },
            });
            var dataTable = new CreatureTypeInfo.CreatureDataTable();
            dataTable.dictionary["name"] = nameList;
            var riskList = new CreatureTypeInfo.CreatureDataList { itemName = "riskLevel" };
            GameBridge.Set(riskList, "list", new List<CreatureTypeInfo.CreatureData>
            {
                new CreatureTypeInfo.CreatureData { data = "ZAYIN", openLevel = 0 },
            });
            dataTable.dictionary["riskLevel"] = riskList;
            GameBridge.Set(metaInfo, "dataTable", dataTable);

            metaInfo.workProbTable = new CreatureWorkProbTable(); // 默认构造即全零表
            configure(metaInfo.workProbTable);
            metaInfo.feelingStateCubeBounds = new FeelingStateCubeBounds { upperBounds = new[] { 6, 9, 12 } };
            metaInfo.workDamage = new DamageInfo(RwbpType.W, 5, 10);
            GameBridge.Set(creature, "metaInfo", metaInfo);

            return new ProbeExt(creature);
        }

        private static GlobalGameManagerReset UnlimitedMode()
        {
            var ggm = GameBridge.Bare<GlobalGameManager>();
            ggm.gameMode = GameMode.UNLIMIT_MODE;
            typeof(GlobalGameManager)
                .GetField("_instance", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .SetValue(null, ggm);
            return new GlobalGameManagerReset();
        }

        private sealed class GlobalGameManagerReset : IDisposable
        {
            public void Dispose()
            {
                typeof(GlobalGameManager)
                    .GetField("_instance", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                    .SetValue(null, null);
            }
        }
    }
}
