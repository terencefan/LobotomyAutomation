using System;
using System.Collections;
using System.Reflection;

namespace AutoInority.Creature
{
    internal class YouMustBeHappyExt : ChannelKitExt
    {
        private const int MaxAutoUses = 5;

        /// <summary>YES 等待期（至多一个翻转周期）内 elapsed²/6 的增长余量。</summary>
        private const int SafetyMargin = 3;

        /// <summary>硬顶哨位比死线提前的缓冲：一帧内 v 增长 &lt;1，取 1 已含余量。</summary>
        private const int HardBuffer = 1;

        /// <summary>游戏判 ≥200 进日结死亡名单，故上限取 199。</summary>
        private const int DeathLine = 199;

        private readonly FieldInfo _succeedUsing = typeof(YouMustHappy).GetField(nameof(_succeedUsing), BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly FieldInfo _elapsedUsingTime = typeof(YouMustHappy).GetField(nameof(_elapsedUsingTime), BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly FieldInfo _usedAgentList = typeof(YouMustHappy).GetField("_usedAgentList", BindingFlags.NonPublic | BindingFlags.Instance);

        public YouMustBeHappyExt(CreatureModel kit) : base(kit)
        {
        }

        /// <summary>
        /// buff 量 = elapsed²/6（凸性），停早了白跑一趟；须等达到按员工四维缺口
        /// 算出的目标时长、且当前符号为 YES 才停。游戏翻转只在自身 update 里发生，
        /// CancelWork 同步结算——检查时的 YES 即结算符号，无竞态。
        /// 唯一例外是硬顶：逼近融毁死线（Σ min(v, gap) ≥ 200）时无条件停——
        /// 极端情况下落 NO 吃小负 buf，也好过 YES 结算进日结死亡名单。
        /// </summary>
        public override bool CanStop()
        {
            var skill = _kit.currentSkill;
            if (skill == null || skill.agent == null)
            {
                return (bool)_succeedUsing.GetValue(Script);
            }
            var gaps = StatGaps(skill.agent);
            var elapsed = (float)_elapsedUsingTime.GetValue(Script);
            if (elapsed >= (float)Math.Sqrt(6f * (HardCapValue(gaps) - HardBuffer)))
            {
                return true;
            }
            if (!(bool)_succeedUsing.GetValue(Script))
            {
                return false;
            }
            return elapsed >= (float)Math.Sqrt(6f * TargetBuff(gaps));
        }

        public override bool CanUse(AgentModel agent)
        {
            // 同一员工第 6 次使用即失控死亡（OnReleaseWork usedCount>=6 → LoseControl+Dead）；
            // 计数每日清零且不分派发者，自动化放行到 5 次、第 6 次绝不经自动化之手
            if (!UsageWithinLimit(agent))
            {
                return false;
            }
            // buff 替换制（ONLY_ONE）：旧值已 ≥ 新掷保底时重掷无增益，省次数；
            // 负 buff（暂停/手动在 NO 相位结算所致）放行救援重掷
            var old = agent.GetUnitBufByType(UnitBufType.YOU_MUST_HAPPY) as UnitStatBuf;
            if (old == null)
            {
                return true;
            }
            var stat = old.primaryStat;
            var oldSum = stat.hp + stat.mental + stat.battle + stat.work;
            if (oldSum < 0)
            {
                return true;
            }
            var gaps = StatGaps(agent);
            return oldSum < SumCapped(TargetBuff(gaps), gaps);
        }

        /// <summary>
        /// 目标 buff 量：v* = 使 Σ min(v+SafetyMargin, gap) ≤ DeathLine 的最大 v，
        /// 上界 maxGap。全维 ≥100 的员工 gap 全 0 → v*=0，CanStop 退化为首-YES 停。
        /// </summary>
        internal static int TargetBuff(int[] gaps)
        {
            var maxGap = 0;
            foreach (var g in gaps)
            {
                if (g > maxGap)
                {
                    maxGap = g;
                }
            }
            var best = 0;
            for (var v = 1; v <= maxGap; v++)
            {
                if (SumCapped(v + SafetyMargin, gaps) <= DeathLine)
                {
                    best = v;
                }
                else
                {
                    break;
                }
            }
            return best;
        }

        /// <summary>
        /// 融毁死线对应的 buf 值：最大 v 使 Σ min(v, gap) ≤ 199。缺口总和 ≤199 时
        /// 无论多久都不过载，返回 int.MaxValue（无硬顶）；存在硬顶时必 ≥49
        /// （Σ min(49, gap) ≤ 196 恒成立）。
        /// </summary>
        internal static int HardCapValue(int[] gaps)
        {
            var maxGap = 0;
            foreach (var g in gaps)
            {
                if (g > maxGap)
                {
                    maxGap = g;
                }
            }
            for (var v = 1; v <= maxGap; v++)
            {
                if (SumCapped(v, gaps) > DeathLine)
                {
                    return v - 1;
                }
            }
            return int.MaxValue;
        }

        internal static int SumCapped(int v, int[] gaps)
        {
            var sum = 0;
            foreach (var g in gaps)
            {
                sum += Math.Min(v, g);
            }
            return sum;
        }

        /// <summary>
        /// 结算时点镜像：游戏 OnReleaseWork 先剥离旧幸福 buf 再按 stat 算 clamp，
        /// 故 gap = 100 − (有效stat − 旧buf值)。维度映射 hp↔fortitude/mental↔prudence/
        /// battle↔justice/work↔temperance（AgentModel 属性定义）。
        /// </summary>
        private static int[] StatGaps(AgentModel agent)
        {
            var old = agent.GetUnitBufByType(UnitBufType.YOU_MUST_HAPPY) as UnitStatBuf;
            var hp = 0;
            var mn = 0;
            var bt = 0;
            var wk = 0;
            if (old != null)
            {
                hp = old.primaryStat.hp;
                mn = old.primaryStat.mental;
                bt = old.primaryStat.battle;
                wk = old.primaryStat.work;
            }
            return new[]
            {
                Math.Max(0, 100 - (agent.fortitudeStat - hp)),
                Math.Max(0, 100 - (agent.prudenceStat - mn)),
                Math.Max(0, 100 - (agent.justiceStat - bt)),
                Math.Max(0, 100 - (agent.temperanceStat - wk)),
            };
        }

        private bool UsageWithinLimit(AgentModel agent)
        {
            var list = _usedAgentList.GetValue(Script) as IEnumerable;
            if (list == null)
            {
                return true;
            }
            foreach (var entry in list)
            {
                var usedAgent = entry.GetType().GetField("usedAgent").GetValue(entry) as AgentModel;
                if (usedAgent != agent)
                {
                    continue;
                }
                return (int)entry.GetType().GetField("usedCount").GetValue(entry) < MaxAutoUses;
            }
            return true;
        }
    }
}
