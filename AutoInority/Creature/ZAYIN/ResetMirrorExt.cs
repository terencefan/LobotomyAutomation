namespace AutoInority.Creature
{
    using System.Collections.Generic;
    using System.Reflection;

    internal class ResetMirrorExt : BaseKitExt
    {
        private const int SpecialtyBound = 65;

        private readonly FieldInfo _alreadyUseAgents = typeof(ResetMirror).GetField("_alreadyUseAgents", BindingFlags.NonPublic | BindingFlags.Instance);

        public ResetMirrorExt(CreatureModel kit) : base(kit)
        {
        }

        public override bool CanUse(AgentModel agent)
        {
            // 同日第二次上镜再 -20 点（OnFinishWork 重复惩罚），自动化不碰
            var used = _alreadyUseAgents.GetValue(Script) as List<AgentModel>;
            if (used != null && used.Contains(agent))
            {
                return false;
            }
            // 重掷=15/15/15/15 基础上随机分配「旧总和-60」：期望守恒的纯方差赌局，
            // 有 4 级以上特长（任一维 >=65）的员工不该赌
            return !HasSpecialty(agent);
        }

        private static bool HasSpecialty(AgentModel agent)
        {
            var stat = agent.primaryStat;
            return stat.hp >= SpecialtyBound || stat.mental >= SpecialtyBound
                || stat.battle >= SpecialtyBound || stat.work >= SpecialtyBound;
        }
    }
}
