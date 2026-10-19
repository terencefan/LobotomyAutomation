using System.Linq;
using System.Reflection;

using AutoInority;
using AutoInority.Extentions;

namespace AutoInority.Creature
{
    internal class ResearcherNoteExt : EquipKitExt
    {
        // 游戏自爆线：持有期间物+神累计实际损血（护甲结算后净值）>60 即死并 30R
        // 溅射（ResearcherNote.cs L200-216）。计数器只在取件时清零，回复不减免，
        // B 伤害双计（WorkerModel.cs 黑伤同时进物/神两事件）——白嫖 +20 自律以
        // 此预算为界
        private const float DamageLimit = 60f;

        // 解锁且满 30s 后，预算余量低于此值才归还：余量尚足就继续持有干活
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

        // 持有期间物+神累计伤害 >60（_gainedDamage）立即自爆并 30R 溅射
        // （ResearcherNote.cs L200-216）：只交给 HP 与精神均 >=80% 的职员，
        // 把吃满 60 伤害的余量拉到最长
        public override bool CanUse(AgentModel agent)
        {
            return agent.hp >= agent.maxHp * 0.8f && agent.mental >= agent.maxMental * 0.8f;
        }

        public override bool CanReturn(AgentModel agent)
        {
            var elapsed = (float)_equipElapsedTime.GetValue(Script.kitEvent);
            var managed = (bool)_creatureManaged.GetValue(Script.kitEvent);
            // 解锁且满 30s 后不急着还：预算余量充足就继续持有白嫖，见底才归还。
            // 该判据单调（managed/elapsed 只增、预算只降），一旦可还永远可还
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

        // 伤害预算下的存活判定：预算池 = 60 − 已吃伤害，同一套 Survive 数学卷积；
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

            var managed = (bool)_creatureManaged.GetValue(Script.kitEvent);
            if (!managed && agent.IsAvailable())
            {
                // 解锁工只挑 ZAYIN/TETH：持有期每一点伤害都吃预算，HE 起就太贵；
                // 派工本身经 Candidate.Suggest → CheckSurvive，预算不安全的解锁工到不了手
                var creatures = CreatureManager.instance.GetCreatureList().Where(x => x.IsAvailable() && x.IsCreature() && x.GetRiskLevel() <= 2);
                var candidates = Candidate.Suggest(agent, creatures);
                if (candidates.Any())
                {
                    candidates.Sort(Comparer);
                    candidates.First().Apply();
                }
            }

            // 不走 EquipKitExt 缺省归还调度：其 IsAvailable（≥90% 血精）门槛会让
            // 干活掉血的持有者归还永久停摆；且笔记 CanReturn 单调，无需取消分支
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

        private static int Comparer(Candidate x, Candidate y) => x.Distance.CompareTo(y.Distance);
    }
}
