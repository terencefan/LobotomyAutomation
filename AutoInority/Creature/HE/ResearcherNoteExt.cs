using System.Reflection;

using AutoInority;
using AutoInority.Extentions;

namespace AutoInority.Creature
{
    internal class ResearcherNoteExt : EquipKitExt
    {
        // 游戏自爆线：持有期间物+神累计实际损血（护甲结算后净值）>60 即死并 30R
        // 溅射（ResearcherNote.cs L200-216）。计数器只在取件时清零，回复不减免，
        // B 伤害双计（WorkerModel.cs 黑伤同时进物/神两事件）
        private const float DamageLimit = 60f;

        // 已解锁且满 30s 后，预算余量低于此值即自动归还：防手动持有的员工贴着自爆线继续干活
        private const float ReturnBelow = 20f;

        private readonly FieldInfo _creatureManaged = typeof(ResearcherNote.ResearcherNoteKit).GetField(
            nameof(_creatureManaged),
            BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly FieldInfo _equipElapsedTime = typeof(ResearcherNote.ResearcherNoteKit).GetField(
                    nameof(_equipElapsedTime),
            BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo _gainedDamage = typeof(ResearcherNote.ResearcherNoteKit).GetField(
            nameof(_gainedDamage),
            BindingFlags.NonPublic | BindingFlags.Instance);

        public ResearcherNoteExt(CreatureModel kit) : base(kit)
        {
        }

        // 自动化永不主动取件：随身收益只有 +20 自律，抵不上持有期物+神累计 >60
        // 自爆（并 30R 溅射友军）的风险。笔记只对玩家手动取件的员工生效，
        // 本类其余逻辑（预算闸/归还调度/镇压排除）全部转为保护该手动持有者
        public override bool CanUse(AgentModel agent) => false;

        public override bool CanReturn(AgentModel agent)
        {
            var elapsed = (float)_equipElapsedTime.GetValue(Script.kitEvent);
            var managed = (bool)_creatureManaged.GetValue(Script.kitEvent);
            // 游戏安全归还条件（解锁+满 30s）满足后，预算余量见底即归还。
            // 判据单调（managed/elapsed 只增、预算只降），一旦可还永远可还
            return managed && elapsed > 30 && DamageBudget(agent) < ReturnBelow;
        }

        // 60 自爆线折算的剩余预算：非持有者/持他 kit → MaxValue（预算闸恒过）
        internal static float DamageBudget(AgentModel agent)
        {
            var equipment = agent.Equipment;
            var kit = equipment == null ? null : equipment.kitCreature;
            if (kit == null || !(kit.script is ResearcherNote))
            {
                return float.MaxValue;
            }
            return DamageLimit - (float)_gainedDamage.GetValue(kit.script.kitEvent);
        }

        // 伤害预算下的存活判定（被动保险：只对手动持有者收紧自动化派工）：
        // 预算池 = 60 − 已吃伤害，同一套 Survive 数学卷积；
        // B 伤害物/神双计（WorkerModel.cs L477+L482 同一笔进两次），预算池折半
        internal static bool CheckBudgetSurvive(AgentModel agent, float minDamage, float maxDamage,
            double workSuccessProb, int maxCubeCount, RwbpType type)
        {
            var budget = DamageBudget(agent);
            if (budget == float.MaxValue)
            {
                return true;
            }
            if (type == RwbpType.B)
            {
                budget /= 2f;
            }
            return Confidence.Survive(budget, minDamage, maxDamage, (float)workSuccessProb, maxCubeCount) > AutomationConfig.DeadConfidence;
        }

        public override void OnFixedUpdate()
        {
            var agent = _kit.kitEquipOwner;
            if (agent == null)
            {
                return;
            }

            // 不走 EquipKitExt 缺省归还调度：其 IsAvailable（≥90% 血精）门槛会让
            // 干活掉血的手动持有者归还永久停摆；且笔记 CanReturn 单调，无需取消分支
            if (CanReturn(agent))
            {
                Return(agent);
            }
        }

        // 归还动作（测试 seam：ReturnKitCreature 摸游戏命令队列）
        protected virtual void Return(AgentModel agent)
        {
            Angela.Log($"已安排{agent.Tag()}归还{_kit.Tag()}");
            agent.ReturnKitCreature();
        }
    }
}
