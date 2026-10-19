using System.Collections.Generic;

using AutoInority.Creature;
using AutoInority.Extentions;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 研究员笔记（ResearcherNote，kit 300002）健康闸：安全归还=持有 ≥30s 且完成
    /// 指定工作（_equipElapsedTime>=30 && _creatureManaged，mod CanReturn 判定一致）；
    /// 持有期间物+神累计伤害 >60（_gainedDamage）立即自爆并 30R 溅射
    /// （ResearcherNote.cs L200-216）。新增 CanUse：HP 与精神均 ≥80% 的职员才接本，
    /// 把吃满 60 伤害的余量拉到最长。浮点说明：0.8f 阈值在 hp=80/maxHp=100 处为
    /// 79.999…，夹线取 75/85 避开噪声区。
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
    }
}
