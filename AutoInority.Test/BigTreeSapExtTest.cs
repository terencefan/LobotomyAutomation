using System.Collections.Generic;

using AutoInority.Creature;
using AutoInority.Extentions;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 巨树汁液（T-09-88 巨树汁液：BigTreeSap，kit 300005）启用：完工回满 HP+60s
    /// 每秒回血（BigTreeSapHeal buf），但爆炸概率=(explodeLevel-1)*15%（封顶 60%），
    /// 计数全设施共享、每用 +1、每阶段重置 1（BigTreeSap.cs L15-48）；爆炸=BigTreeSapBuf
    /// 引爆处决。三闸启用（用户口径）：仅紧急事件中（Automaton.InEmergency：考验在场
    /// 或存在过载收容物）、仅 hp&lt;15% 濒死职员、仅每阶段首用（explodeLevel==1，此时
    /// 爆炸概率 0%）。用完即还（收益在完工瞬间，持有无价值）。
    /// InEmergency 依赖两个游戏管理器懒汉单例：OrdealManager.ordealCreatureList 声明即
    /// 初始化（空=true 安全），CreatureManager.creatureList 为 null 必须注入空表。
    /// </summary>
    [Collection("GameBridge")] // GetKitExtension 静态缓存 + 管理器单例，跨类串行
    public class BigTreeSapExtTest
    {
        public BigTreeSapExtTest()
        {
            // OrdealManager/CreatureManager 是静态懒汉单例，跨测试残留，逐用例重置
            GameBridge.Set(CreatureManager.instance, "creatureList", new List<CreatureModel>());
            GameBridge.Set(OrdealManager.instance, "ordealCreatureList", new List<OrdealCreatureModel>());
        }

        private static CreatureModel Kit(int explodeLevel)
        {
            var kit = GameBridge.Bare<CreatureModel>();
            var script = GameBridge.Bare<BigTreeSap>();
            GameBridge.Set(script, "explodeLevel", explodeLevel);
            GameBridge.Set(kit, "script", script);
            var metaInfo = GameBridge.Bare<CreatureTypeInfo>();
            var nameList = new CreatureTypeInfo.CreatureDataList { itemName = "name" };
            GameBridge.Set(nameList, "list", new List<CreatureTypeInfo.CreatureData>
            {
                new CreatureTypeInfo.CreatureData { data = "巨树汁液", openLevel = 0 },
            });
            var dataTable = new CreatureTypeInfo.CreatureDataTable();
            dataTable.dictionary["name"] = nameList;
            GameBridge.Set(metaInfo, "dataTable", dataTable);
            // IsKit 走 creatureWorkType（缺省 NORMAL）
            GameBridge.Set(metaInfo, "creatureWorkType", CreatureWorkType.KIT);
            GameBridge.Set(kit, "metaInfo", metaInfo);
            return kit;
        }

        private static AgentModel DyingAgent(int hp = 10, int maxHp = 100)
        {
            var agent = GameBridge.CanWorkAgent("小红");
            // AgentModel.maxHp = primaryStat.maxHP(=> hp) + buf/EGO/title 各加成；
            // 空表注入后即 maxHp
            var stat = GameBridge.Bare<WorkerPrimaryStat>();
            GameBridge.Set(stat, "hp", maxHp);
            GameBridge.Set(agent, "primaryStat", stat);
            GameBridge.Set(agent, "hp", hp);
            GameBridge.Set(agent, "_statBufList", new List<UnitStatBuf>());
            return agent;
        }

        private static void EnterEmergency()
        {
            var ordeal = GameBridge.Bare<OrdealCreatureModel>();
            GameBridge.Set(ordeal, "_state", CreatureState.WAIT);
            GameBridge.Set(OrdealManager.instance, "ordealCreatureList", new List<OrdealCreatureModel> { ordeal });
        }

        [Fact]
        public void KitFactoryResolvesBigTreeSapBranch()
        {
            // 修复钉：原工厂给 NeverUseKitExt，巨树汁液永久禁用
            Assert.IsType<BigTreeSapExt>(Kit(1).GetKitExtension());
        }

        [Fact]
        public void CanUseInEmergencyForDyingAgentAtFirstUse()
        {
            EnterEmergency();
            var ext = new BigTreeSapExt(Kit(1));
            Assert.True(ext.CanUse(DyingAgent()));
        }

        [Fact]
        public void CanUseRejectedOutsideEmergency()
        {
            var ext = new BigTreeSapExt(Kit(1));
            Assert.False(ext.CanUse(DyingAgent()));
        }

        [Fact]
        public void CanUseRejectedForHealthyAgent()
        {
            // hp=20（20%）远高于 15% 线：拒（15 整数恰在浮点阈值 14.999… 的噪声区，不作夹线）
            EnterEmergency();
            var ext = new BigTreeSapExt(Kit(1));
            Assert.False(ext.CanUse(DyingAgent(hp: 20)));
        }

        [Fact]
        public void CanUseRejectedAfterFirstUse()
        {
            // explodeLevel=2：本阶段已首用过，爆炸概率 15%，不再启用
            EnterEmergency();
            var ext = new BigTreeSapExt(Kit(2));
            Assert.False(ext.CanUse(DyingAgent()));
        }

        [Fact]
        public void CanReturnAlwaysTrue()
        {
            EnterEmergency();
            var ext = new BigTreeSapExt(Kit(1));
            Assert.True(ext.CanReturn(DyingAgent()));
        }
    }
}
