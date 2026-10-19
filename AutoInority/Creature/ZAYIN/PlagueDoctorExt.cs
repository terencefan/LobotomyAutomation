namespace AutoInority.Creature
{
    using WhiteNightSpace;

    internal class PlagueDoctorExt : ExpectNormalExt
    {
        public override SkillTypeInfo[] SkillSets => new SkillTypeInfo[] { Insight, Attachment, Repression };

        public PlagueDoctorExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            var doctor = Script as PlagueDoctor;
            // 使徒再派工：游戏只在 UI 层拦截（BlessedAgentWorkCheck），自动化派工路径需自查
            if (doctor != null && doctor.CheckApostle(agent))
            {
                message = Message(Angela.Creature.PlagueDoctorApostle, agent, skill);
                return false;
            }
            // 第 11 次祝福后，任何 GOOD 结果都会凑满 12 触发白夜降临（AdventLevel=12）
            if (doctor != null && doctor.ApostleLevel >= 11)
            {
                message = Message(Angela.Creature.PlagueDoctor, agent, skill);
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }
    }
}
