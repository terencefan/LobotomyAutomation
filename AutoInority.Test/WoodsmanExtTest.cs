using System.Collections.Generic;

using AutoInority.Creature;
using AutoInority.Extentions;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 樵夫（O-05-30 无题：Lumberjack）双修：①扣计数条件=完成工作且
    /// temperanceLevel&gt;=3（Lumberjack.cs L145-156），mod 原用 prudenceLevel&gt;2
    /// 张冠李戴 → 改 temperanceLevel&gt;2 限定过载；②workProb 沟通(B) 0.5-0.9
    /// 遥遥领先、其余 0.45 → SkillSets 收窄 {Attachment}。counter==0 进门=绑架
    /// 处决（L130-143），既有拒绝维持。temperanceLevel 链 =
    /// CalculateStatLevel(primaryStat.work)，阈值 30/45/65/85（30 → 2 级、50 → 3 级）。
    /// 基线路径经 ScriptedWoodsmanExt 注入工作成功率（管道级联游戏数据，离线不可驱动，
    /// 先例 ShyLookDoorTest.ProbAtDoor）。
    /// </summary>
    [Collection("GameBridge")] // GetExtension 静态缓存 + SkillTypeList 单例，跨类串行
    public class WoodsmanExtTest
    {
        private sealed class ScriptedWoodsmanExt : WoodsmanExt
        {
            private readonly float _prob;

            internal ScriptedWoodsmanExt(CreatureModel creature, float prob) : base(creature)
            {
                _prob = prob;
            }

            protected override float CalculateWorkSuccessProb(AgentModel agent, SkillTypeInfo skill) => _prob;
        }

        private static CreatureModel Creature(bool overloaded = false)
        {
            var creature = GameBridge.Bare<CreatureModel>();
            GameBridge.Set(creature, "script", GameBridge.Bare<Lumberjack>());
            GameBridge.Set(creature, "isOverloaded", overloaded);
            GameBridge.Set(creature, "_qliphothCounter", 2);
            var metaInfo = GameBridge.Bare<CreatureTypeInfo>();
            // name property 走 dataTable 真数据管道，未知 key 触发 Debug.LogError icall（先例 LadyLookingAtWallTest）
            var nameList = new CreatureTypeInfo.CreatureDataList { itemName = "name" };
            GameBridge.Set(nameList, "list", new List<CreatureTypeInfo.CreatureData>
            {
                new CreatureTypeInfo.CreatureData { data = "无题", openLevel = 0 },
            });
            var riskList = new CreatureTypeInfo.CreatureDataList { itemName = "riskLevel" };
            GameBridge.Set(riskList, "list", new List<CreatureTypeInfo.CreatureData>
            {
                new CreatureTypeInfo.CreatureData { data = "HE", openLevel = 0 },
            });
            var dataTable = new CreatureTypeInfo.CreatureDataTable();
            dataTable.dictionary["name"] = nameList;
            dataTable.dictionary["riskLevel"] = riskList;
            GameBridge.Set(metaInfo, "dataTable", dataTable);
            // 真实界值形态：NormalBound 6 / GoodBound 9 / MaxCube 12（先例 ShyLookDoorTest）
            metaInfo.feelingStateCubeBounds = new FeelingStateCubeBounds { upperBounds = new[] { 6, 9, 12 } };
            GameBridge.Set(creature, "metaInfo", metaInfo);
            return creature;
        }

        private static AgentModel Agent(int temperanceStat, int prudenceStat)
        {
            var agent = GameBridge.CanWorkAgent("小红");
            var stat = GameBridge.Bare<WorkerPrimaryStat>();
            GameBridge.Set(stat, "work", temperanceStat);
            GameBridge.Set(stat, "mental", prudenceStat);
            GameBridge.Set(agent, "primaryStat", stat);
            // temperance/prudenceLevel 链：GetPrimaryStatBuf 遍历 _statBufList（独立于 _bufList）
            GameBridge.Set(agent, "_statBufList", new List<UnitStatBuf>());
            return agent;
        }

        private static SkillTypeInfo Skill()
        {
            return new SkillTypeInfo { id = 1, name = "R", calledName = "本能" };
        }

        [Fact]
        public void FactoryResolvesHeBranch()
        {
            GameBridge.InjectSkillTypes();
            Assert.IsType<WoodsmanExt>(Creature().GetExtension());
        }

        [Fact]
        public void SkillSetsOnlyAttachment()
        {
            // workProb 沟通 0.5-0.9 独大（其余 0.45），只派沟通
            GameBridge.InjectSkillTypes();
            var sets = new WoodsmanExt(Creature()).SkillSets;
            var single = Assert.Single(sets);
            Assert.Equal(RwbpType.B, single.rwbpType);
        }

        [Fact]
        public void HighTemperanceRestrictsToOverloaded()
        {
            // temperance 3 级（work=50）+ 非过载：完成工作即扣计数 → 不派
            GameBridge.InjectSkillTypes();
            var ext = new WoodsmanExt(Creature(overloaded: false));
            Assert.False(ext.CheckWorkConfidence(Agent(50, 10), Skill()));
        }

        [Fact]
        public void HighTemperanceOverloadedPasses()
        {
            GameBridge.InjectSkillTypes();
            var ext = new WoodsmanExt(Creature(overloaded: true));
            Assert.True(ext.CheckWorkConfidence(Agent(50, 10), Skill()));
        }

        [Fact]
        public void TemperanceBoundaryLevel2TakesBasePath()
        {
            // temperance 2 级（work=30）+ prudence 3 级（mental=50）：旧 prudence 闸会
            // 误限为仅过载（false），新 temperance 闸走基线 —— p=0.9 的 P(≥7)≈0.9996
            // 大于两道阈值（0.95/0.99），应放行
            GameBridge.InjectSkillTypes();
            var ext = new ScriptedWoodsmanExt(Creature(overloaded: false), 0.9f);
            Assert.True(ext.CheckWorkConfidence(Agent(30, 50), Skill()));
        }

        [Fact]
        public void BasePathLowProbStillRejects()
        {
            // p=0.2 的 P(≥7)≈0.0004：基线置信不过线照旧弹回
            GameBridge.InjectSkillTypes();
            var ext = new ScriptedWoodsmanExt(Creature(overloaded: false), 0.2f);
            Assert.False(ext.CheckWorkConfidence(Agent(30, 10), Skill()));
        }
    }
}
