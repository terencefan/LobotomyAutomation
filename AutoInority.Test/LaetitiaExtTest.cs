using System.Collections.Generic;

using AutoInority.Creature;
using AutoInority.Extentions;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 心之归属地规则收敛（蕾蒂希娅）：机制在基类、数据在 creature——
    /// AcceptsHeartAgent 默认拒绝、LaetitiaExt 放行，调度层经 CreatureModelExtensions
    /// 查询不感知具体类型（类型知识收敛回工厂映射）。派工闸（CanWorkWith 首行，
    /// 约束自动化派工）与门口兜底（ViableAtDoor 总入口组合 DoorCheck，拦手工误派——
    /// 游戏对带心员工派往他处只点燃 5~10s 引信不阻止派工，覆写 DoorCheck 无法绕过）
    /// 分开直驱。文案断言钉住指代修正：「害羞的小孩」字面量替代 {1}（收容物名占位
    /// 会把「蕾蒂希娅」填进礼物归属），兼作 mojibake 回归。
    /// </summary>
    [Collection("GameBridge")] // GetExtension 静态缓存 + SkillTypeList 单例，跨类串行
    public class LaetitiaExtTest
    {
        private sealed class PlainExt : BaseCreatureExt
        {
            internal PlainExt(CreatureModel creature) : base(creature)
            {
            }
        }

        private static CreatureModel Creature(string riskLevel = null)
        {
            var creature = GameBridge.Bare<CreatureModel>();
            GameBridge.Set(creature, "script", GameBridge.Bare<LittleWitch>());
            var metaInfo = GameBridge.Bare<CreatureTypeInfo>();
            // name property 走 dataTable → CreatureDataList 游戏真数据管道；
            // 未知 key 触发 Debug.LogError 的引擎 icall，须注入真名条目绕开
            var nameList = new CreatureTypeInfo.CreatureDataList { itemName = "name" };
            GameBridge.Set(nameList, "list", new List<CreatureTypeInfo.CreatureData>
            {
                new CreatureTypeInfo.CreatureData { data = "蕾蒂希娅", openLevel = 0 },
            });
            var dataTable = new CreatureTypeInfo.CreatureDataTable();
            dataTable.dictionary["name"] = nameList;
            GameBridge.Set(metaInfo, "dataTable", dataTable);
            // 纯字符串解析链（riskLevelForce 属性 → dataTable["riskLevel"] 首条 data
            // → 枚举 +1），HE 评级驱动工厂映射
            if (riskLevel != null)
            {
                var riskList = new CreatureTypeInfo.CreatureDataList { itemName = "riskLevel" };
                GameBridge.Set(riskList, "list", new List<CreatureTypeInfo.CreatureData>
                {
                    new CreatureTypeInfo.CreatureData { data = riskLevel, openLevel = 0 },
                });
                dataTable.dictionary["riskLevel"] = riskList;
            }
            GameBridge.Set(creature, "metaInfo", metaInfo);
            return creature;
        }

        private static AgentModel AgentWithHeart()
        {
            var agent = GameBridge.CanWorkAgent("小红");
            var heart = GameBridge.Bare<UnitBuf>();
            heart.type = UnitBufType.LITTLEWITCH_HEART;
            ((List<UnitBuf>)GameBridge.Get(agent, "_bufList")).Add(heart);
            return agent;
        }

        private static SkillTypeInfo Skill()
        {
            return new SkillTypeInfo { id = 3, name = "B", calledName = "本能" };
        }

        [Fact]
        public void OnlyLaetitiaAcceptsHeartAgent()
        {
            GameBridge.InjectSkillTypes();
            Assert.True(new LaetitiaExt(Creature()).AcceptsHeartAgent);
            Assert.False(new PlainExt(Creature()).AcceptsHeartAgent);
        }

        [Fact]
        public void HelperResolvesThroughFactory()
        {
            // 类型知识收敛回工厂映射：script=LittleWitch + HE 评级 → LaetitiaExt，
            // 调度层 helper 只见 bool
            GameBridge.InjectSkillTypes();
            Assert.True(Creature("HE").AcceptsHeartAgent());
        }

        [Fact]
        public void CanWorkWithRejectsHeartAgentOutsideHome()
        {
            GameBridge.InjectSkillTypes();
            Assert.False(new PlainExt(Creature()).CanWorkWith(AgentWithHeart(), Skill(), out var message));
            Assert.NotNull(message);
            Assert.Contains("害羞的小孩", message);
            Assert.Contains("小红", message);
            // 指代修正的反向钉：旧模板 {1} 会把拒绝目标名（蕾蒂希娅）填进礼物归属，现已整体移除
            Assert.DoesNotContain("蕾蒂希娅", message);
        }

        [Fact]
        public void CanWorkWithPassesHeartAgentAtHome()
        {
            // 归属地放行：心之行让路，后续检查点（精灵祝福/破甲/好友币）由注入短路
            GameBridge.InjectSkillTypes();
            Assert.True(new LaetitiaExt(Creature()).CanWorkWith(AgentWithHeart(), Skill(), out var message));
            Assert.Null(message);
        }

        [Fact]
        public void DoorGateBouncesHeartAgentOutsideHome()
        {
            // 手工误派兜底：DoorCheck 默认放行的 creature 也拦——总入口先于心检
            GameBridge.InjectSkillTypes();
            Assert.False(new PlainExt(Creature()).ViableAtDoor(AgentWithHeart(), Skill(), out var message));
            Assert.NotNull(message);
            Assert.Contains("害羞的小孩", message);
            Assert.Contains("小红", message);
        }

        [Fact]
        public void DoorGatePassesHeartAgentAtHome()
        {
            GameBridge.InjectSkillTypes();
            Assert.True(new LaetitiaExt(Creature()).ViableAtDoor(AgentWithHeart(), Skill(), out _));
        }
    }
}
