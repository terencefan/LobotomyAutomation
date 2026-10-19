using System.Collections.Generic;
using System.Reflection;

using AutoInority.Creature;
using AutoInority.Extentions;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 研究员笔记（ResearcherNote，kit 300002）：安全归还=持有 ≥30s 且完成指定工作
    /// （_equipElapsedTime>=30 && _creatureManaged，与游戏判定一致）；持有期间物+神
    /// 累计伤害 >60（_gainedDamage）立即自爆并 30R 溅射（ResearcherNote.cs L200-216）。
    /// 伤害预算方案：CanUse 取件闸 ≥80%（浮点夹线取 75/85 避开 0.8f 噪声区）；
    /// CheckSurvive 对持有者把 60 线当第二血池再过一遍 Survive 数学（B 双计折半）；
    /// CanReturn 在解锁+30s 后以预算余量 <20 才归还（白嫖 +20 自律到见底为止）。
    /// </summary>
    [Collection("GameBridge")] // GetKitExtension 静态缓存，跨类串行
    public class ResearcherNoteExtTest
    {
        private static CreatureModel Kit()
        {
            var kit = GameBridge.Bare<CreatureModel>();
            GameBridge.Set(kit, "script", GameBridge.Bare<ResearcherNote>());
            var metaInfo = GameBridge.Bare<CreatureTypeInfo>();
            var nameList = new CreatureTypeInfo.CreatureDataList { itemName = "name" };
            GameBridge.Set(nameList, "list", new List<CreatureTypeInfo.CreatureData>
            {
                new CreatureTypeInfo.CreatureData { data = "研究员笔记", openLevel = 0 },
            });
            var dataTable = new CreatureTypeInfo.CreatureDataTable();
            dataTable.dictionary["name"] = nameList;
            GameBridge.Set(metaInfo, "dataTable", dataTable);
            // IsKit 走 creatureWorkType（缺省 NORMAL）
            GameBridge.Set(metaInfo, "creatureWorkType", CreatureWorkType.KIT);
            GameBridge.Set(kit, "metaInfo", metaInfo);
            return kit;
        }

        private static AgentModel Agent(int hp, int maxHp, int mental, int maxMental)
        {
            var agent = GameBridge.CanWorkAgent("小红");
            var stat = GameBridge.Bare<WorkerPrimaryStat>();
            // WorkerPrimaryStat：maxHP => hp、maxMental => mental
            GameBridge.Set(stat, "hp", maxHp);
            GameBridge.Set(stat, "mental", maxMental);
            GameBridge.Set(agent, "primaryStat", stat);
            GameBridge.Set(agent, "hp", hp);
            GameBridge.Set(agent, "mental", mental);
            GameBridge.Set(agent, "_statBufList", new List<UnitStatBuf>());
            return agent;
        }

        /// <summary>构造带游戏侧状态的 ResearcherNoteKit（kitEvent），字段直注。</summary>
        private static ResearcherNote.ResearcherNoteKit NoteEvent(float gained, bool managed, float elapsed)
        {
            var kitEvent = new ResearcherNote.ResearcherNoteKit(GameBridge.Bare<ResearcherNote>());
            GameBridge.Set(kitEvent, "_gainedDamage", gained);
            GameBridge.Set(kitEvent, "_creatureManaged", managed);
            GameBridge.Set(kitEvent, "_equipElapsedTime", elapsed);
            return kitEvent;
        }

        private static ResearcherNoteExt ExtWithState(float gained, bool managed, float elapsed, out CreatureModel kit)
        {
            kit = Kit();
            GameBridge.Set(kit.script, "kitEvent", NoteEvent(gained, managed, elapsed));
            return new ResearcherNoteExt(kit);
        }

        /// <summary>持有指定笔记 kit 的员工：直接在 CanWorkAgent 自带装备空间上挂
        /// kitCreature（换新 UnitEquipSpace 会丢 gifts 链，maxHp→GetEGObonus 会炸）。</summary>
        private static AgentModel Holder(int hp, int maxHp, int mental, int maxMental, CreatureModel kit)
        {
            var agent = Agent(hp, maxHp, mental, maxMental);
            GameBridge.Set(agent.Equipment, "kitCreature", kit);
            return agent;
        }

        /// <summary>AgentManager.instance 惰性构造会级联 Init()，直接注入裸单例绕开。</summary>
        private static void SetAgents(params AgentModel[] agents)
        {
            var manager = GameBridge.Bare<AgentManager>();
            GameBridge.Set(manager, "agentList", new List<AgentModel>(agents));
            typeof(AgentManager).GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static)
                .SetValue(null, manager);
        }

        [Fact]
        public void KitFactoryResolvesResearcherNoteBranch()
        {
            Assert.IsType<ResearcherNoteExt>(Kit().GetKitExtension());
        }

        [Fact]
        public void CanUseAcceptsHealthyAgent()
        {
            // 85/100 与 90/100：均超 80% 线
            var ext = new ResearcherNoteExt(Kit());
            Assert.True(ext.CanUse(Agent(85, 100, 90, 100)));
        }

        [Fact]
        public void CanUseRejectsWoundedAgent()
        {
            var ext = new ResearcherNoteExt(Kit());
            Assert.False(ext.CanUse(Agent(75, 100, 95, 100)));
        }

        [Fact]
        public void CanUseRejectsShakenAgent()
        {
            var ext = new ResearcherNoteExt(Kit());
            Assert.False(ext.CanUse(Agent(95, 100, 75, 100)));
        }

        // —— 伤害预算（60 自爆线 = 第二血池）——

        [Fact]
        public void DamageBudgetTracksHeldNote()
        {
            CreatureModel kit;
            ExtWithState(45f, true, 35f, out kit);
            var holder = Holder(90, 100, 90, 100, kit);

            Assert.Equal(15f, ResearcherNoteExt.DamageBudget(holder), 5);
        }

        [Fact]
        public void DamageBudgetInfiniteForNonHolders()
        {
            Assert.Equal(float.MaxValue, ResearcherNoteExt.DamageBudget(Agent(90, 100, 90, 100)));
        }

        [Fact]
        public void DamageBudgetInfiniteForOtherKits()
        {
            var agent = Agent(90, 100, 90, 100);
            var equipment = GameBridge.Bare<UnitEquipSpace>();
            var other = GameBridge.Bare<CreatureModel>();
            GameBridge.Set(other, "script", GameBridge.Bare<Shelter>());
            GameBridge.Set(equipment, "kitCreature", other);
            GameBridge.Set(agent, "_equipment", equipment);

            Assert.Equal(float.MaxValue, ResearcherNoteExt.DamageBudget(agent));
        }

        [Fact]
        public void CheckBudgetSurviveBlackHalves()
        {
            // 预算 30：黑伤物/神双计（WorkerModel.cs L477+L482）折半成 15 池——
            // min=max=20、失败上限 1、成功率 0.5：红伤 30 池可活（两跳各 20 有一跳落空即活），
            // 黑伤 15 池必死
            CreatureModel kit;
            ExtWithState(30f, true, 10f, out kit);
            var holder = Holder(90, 100, 90, 100, kit);

            Assert.True(ResearcherNoteExt.CheckBudgetSurvive(holder, 20f, 20f, 0.5f, 1, RwbpType.R));
            Assert.False(ResearcherNoteExt.CheckBudgetSurvive(holder, 20f, 20f, 0.5f, 1, RwbpType.B));
        }

        [Fact]
        public void CheckBudgetSurviveWithFreshBudget()
        {
            // 预算 60：10 伤害 5 跳全吃也活着
            CreatureModel kit;
            ExtWithState(0f, true, 0f, out kit);
            var holder = Holder(90, 100, 90, 100, kit);

            Assert.True(ResearcherNoteExt.CheckBudgetSurvive(holder, 10f, 10f, 0.5f, 5, RwbpType.R));
        }

        [Fact]
        public void CheckBudgetSurviveIgnoresNonHolders()
        {
            // 非持有者预算闸恒过：致死伤害也由真实血池判定（此处只验预算闸不拦）
            Assert.True(ResearcherNoteExt.CheckBudgetSurvive(Agent(90, 100, 90, 100), 500f, 500f, 0.1f, 1, RwbpType.R));
        }

        // —— 归还策略（白嫖到预算见底）——

        [Fact]
        public void CanReturnWaitsWhileBudgetHealthy()
        {
            CreatureModel kit;
            var ext = ExtWithState(30f, true, 35f, out kit);
            var holder = Holder(90, 100, 90, 100, kit);

            // 解锁+满 30s，但预算余 30 ≥ 20：继续持有白嫖
            Assert.False(ext.CanReturn(holder));
        }

        [Fact]
        public void CanReturnWhenBudgetRunsLow()
        {
            CreatureModel kit;
            var ext = ExtWithState(45f, true, 35f, out kit);
            var holder = Holder(90, 100, 90, 100, kit);

            // 预算余 15 < 20：归还
            Assert.True(ext.CanReturn(holder));
        }

        [Fact]
        public void CanReturnStillRequiresUnlockAndTime()
        {
            CreatureModel kit;
            var ext = ExtWithState(59f, false, 35f, out kit);
            var holder = Holder(90, 100, 90, 100, kit);
            Assert.False(ext.CanReturn(holder)); // 没解锁，余量再低也得先完成工作

            CreatureModel kit2;
            var ext2 = ExtWithState(59f, true, 25f, out kit2);
            var holder2 = Holder(90, 100, 90, 100, kit2);
            Assert.False(ext2.CanReturn(holder2)); // 不到 30s
        }

        // —— OnFixedUpdate 调度 ——

        /// <summary>Return 摸游戏命令队列（ReturnKitCreature），测试记录调用即可。</summary>
        private sealed class RecordingNoteExt : ResearcherNoteExt
        {
            public readonly List<AgentModel> Returned = new List<AgentModel>();

            public RecordingNoteExt(CreatureModel kit) : base(kit)
            {
            }

            protected override void Return(AgentModel agent)
            {
                Returned.Add(agent);
            }
        }

        [Fact]
        public void OnFixedUpdateReturnsWoundedHolder()
        {
            // 掉血到 90% 线下的持有者归还照发：EquipKitExt 缺省调度的 IsAvailable
            // 门槛会让停摆洞卡死持有到日终，此处钉死不走缺省调度
            CreatureModel kit;
            ExtWithState(45f, true, 35f, out kit);
            var recording = new RecordingNoteExt(kit);
            var holder = Holder(50, 100, 90, 100, kit);
            GameBridge.Set(kit, "kitEquipOwner", holder);

            recording.OnFixedUpdate();

            Assert.Same(holder, Assert.Single(recording.Returned));
        }

        [Fact]
        public void OnFixedUpdateHoldsWhileBudgetHealthy()
        {
            CreatureModel kit;
            ExtWithState(30f, true, 35f, out kit);
            var recording = new RecordingNoteExt(kit);
            var holder = Holder(90, 100, 90, 100, kit);
            GameBridge.Set(kit, "kitEquipOwner", holder);

            recording.OnFixedUpdate();

            Assert.Empty(recording.Returned);
        }

        [Fact]
        public void OnFixedUpdateIgnoresUnownedKit()
        {
            CreatureModel kit;
            ExtWithState(59f, true, 35f, out kit);
            var recording = new RecordingNoteExt(kit);
            GameBridge.Set(kit, "kitEquipOwner", null);

            recording.OnFixedUpdate();

            Assert.Empty(recording.Returned);
        }

        // —— 取件池互斥 ——

        [Fact]
        public void FindAgentsExcludesKitHolders()
        {
            // 已持 kit 者不进任何取件池：游戏 SetKitCreature 对已持有者 LogError 空跑
            CreatureModel kit;
            var ext = ExtWithState(0f, false, 0f, out kit);
            var holder = Holder(95, 100, 95, 100, kit);
            var plain = Agent(95, 100, 95, 100);
            SetAgents(holder, plain);

            var found = ext.FindAgents();

            var candidate = Assert.Single(found);
            Assert.Same(plain, candidate);
        }
    }
}
