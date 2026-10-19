using System.Collections.Generic;
using System.Linq;

using AutoInority.Command;
using AutoInority.Extentions;

namespace AutoInority.Posture
{
    /// <summary>
    /// 调度姿态基类：策略切换不清宏/种地状态（WorkingAgents/FarmingCreatures 挂在
    /// Automaton 上，切回自动恢复）。OnTick 只负责向 Automaton 入队，执行段在
    /// Automaton.Main 共用；OnEnter/OnExit 是二期事件驱动自动切换的扩展缝。
    /// </summary>
    internal abstract class PostureBase
    {
        public abstract void OnTick();

        public virtual void OnEnter()
        {
        }

        public virtual void OnExit()
        {
        }

        /// <summary>战斗目标：出逃（AutoSuppress 放行，仅低风险）+ 全部考验。</summary>
        internal static IEnumerable<CreatureModel> GetFightables()
        {
            foreach (var creature in CreatureManager.instance.GetCreatureList().Where(x => x.state == CreatureState.ESCAPE))
            {
                if (creature.GetExtension().AutoSuppress)
                {
                    yield return creature;
                }
            }
            foreach (var creature in OrdealManager.instance.GetOrdealCreatureList())
            {
                yield return creature;
            }
        }

        /// <summary>应急反射：紧急收容物与战斗目标的扫描入队，两姿态共用（自旧 Main 平移）。</summary>
        protected static void EnqueueThreats()
        {
            foreach (var creature in CreatureManager.instance.GetCreatureList().FilterUrgent())
            {
                Automaton.Instance.Enqueue(new ManageCreatureCommand(creature));
            }
            foreach (var creature in GetFightables())
            {
                Automaton.Instance.Enqueue(new SuppressCommand(creature));
            }
        }
    }
}
