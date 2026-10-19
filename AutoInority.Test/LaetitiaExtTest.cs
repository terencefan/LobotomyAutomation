using System.Collections.Generic;
using System.Linq;

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

        private static AgentModel HeartAgent(AgentAIState state, CreatureModel target)
        {
            var agent = AgentWithHeart();
            // state 是 AgentModel 私有属性，背衬字段 _state；target 是 WorkerModel 公有字段
            GameBridge.Set(agent, "_state", state);
            GameBridge.Set(agent, "target", target);
            return agent;
        }

        [Fact]
        public void SuppressionTargetsExcludeDestroyedChilds()
        {
            // GetAliveChilds 不筛死活：destroied 尸体仍报 ESCAPE，不过滤=镇压命令永不完结刷屏
            var creature = Creature();
            var alive = GameBridge.Bare<ChildCreatureModel>();
            var dead = GameBridge.Bare<ChildCreatureModel>();
            dead.destroied = true;
            GameBridge.Set(creature, "childs", new List<ChildCreatureModel> { alive, dead });
            var targets = new LaetitiaExt(creature).SuppressionTargets.ToList();
            var single = Assert.Single(targets);
            Assert.Same(alive, single);
            // 基类默认：无子实体扩展返回空集
            Assert.Empty(new PlainExt(Creature()).SuppressionTargets);
        }

        [Fact]
        public void MainWiresRecallAndChildSuppression()
        {
            // 驱动 Automaton.Main 接线：女巫出逃 + 存活/死亡子实体。HE 本体不自动
            // 镇压是既有姿态策略（AutoSuppress 只到 TETH），子实体不受此限——
            // 只应入队存活子实体一条；带心召回循环走通（AgentManager 留空=无兵
            // 可用，SuppressCommand.Execute 走 repeatQueue 兜底，不碰房间清理）
            var witch = Creature("HE");
            GameBridge.Set(witch, "_state", CreatureState.ESCAPE);
            var alive = GameBridge.Bare<ChildCreatureModel>();
            var dead = GameBridge.Bare<ChildCreatureModel>();
            dead.destroied = true;
            // SuppressCommand 构造查 IsCreature：bare metaInfo 的 creatureWorkType 缺省 NORMAL；
            // 活跃子实体在场上即 ESCAPE（IsCompleted 才不会当完结丢弃）
            GameBridge.Set(alive, "metaInfo", GameBridge.Bare<CreatureTypeInfo>());
            GameBridge.Set(alive, "_state", CreatureState.ESCAPE);
            GameBridge.Set(witch, "childs", new List<ChildCreatureModel> { alive, dead });
            GameBridge.Set(CreatureManager.instance, "creatureList", new List<CreatureModel> { witch });
            GameBridge.Set(AgentManager.instance, "agentList", new List<AgentModel>());
            // 考验懒汉单例跨测试残留（同集合 BigTreeSapExtTest 注入的裸考验体），
            // Main 会遍历考验循环，须隔离清空
            GameBridge.Set(OrdealManager.instance, "ordealCreatureList", new List<OrdealCreatureModel>());

            Automaton.Instance.Main();

            // 无兵可用：入队的镇压命令走 repeatQueue 兜底，等待下一周期
            var repeat = (Queue<AutoInority.Command.ICommand>)GameBridge.Get(Automaton.Instance, "_repeatQueue");
            var single = Assert.Single(repeat);
            Assert.IsType<AutoInority.Command.SuppressCommand>(single);
        }

        [Fact]
        public void ShouldRecallFollowsFuseCondition()
        {
            var witch = Creature();
            var other = Creature();
            // 带心 + MANAGE + 目标非女巫：引信已点燃，须召回
            Assert.True(LaetitiaExt.ShouldRecall(HeartAgent(AgentAIState.MANAGE, other), witch));
            // 目标已是女巫：引信未点燃
            Assert.False(LaetitiaExt.ShouldRecall(HeartAgent(AgentAIState.MANAGE, witch), witch));
            // 非带心员工与引信无关
            var plain = GameBridge.CanWorkAgent("小蓝");
            GameBridge.Set(plain, "_state", AgentAIState.MANAGE);
            GameBridge.Set(plain, "target", other);
            Assert.False(LaetitiaExt.ShouldRecall(plain, witch));
            // 非引导状态（待机/移动中非派工）引信未点燃
            Assert.False(LaetitiaExt.ShouldRecall(HeartAgent(AgentAIState.IDLE, other), witch));
            // 女巫已不在场（无引信源）
            Assert.False(LaetitiaExt.ShouldRecall(HeartAgent(AgentAIState.MANAGE, other), null));
        }

        [Fact]
        public void RecallHeartAgentsSkipsSafeAgents()
        {
            // 仅安全员工：循环与守卫走通，不触发 ForcelyCancelWork
            // （其 MapGraph/房间清理依赖离线不可驱动，真取消在实机验收）
            var witch = Creature();
            var plain = GameBridge.CanWorkAgent("小蓝");
            GameBridge.Set(plain, "_state", AgentAIState.MANAGE);
            GameBridge.Set(plain, "target", witch);
            LaetitiaExt.RecallHeartAgents(new[] { plain, HeartAgent(AgentAIState.IDLE, witch) }, witch);
        }
    }
}
