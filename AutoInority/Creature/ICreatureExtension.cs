namespace AutoInority.Creature
{
    public interface ICreatureExtension
    {
        bool AutoSuppress { get; }

        bool IsUrgent { get; }

        SkillTypeInfo[] SkillSets { get; }

        bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message);

        /// <summary>
        /// 门检：员工抵达工作位、游戏即将创建 UseSkill（UseSkill.InitUseSkillAction）的前一瞬调用，
        /// 返回 false 则拒绝开工并把员工弹回待机通道——此时表情等易变状态刚被定格，
        /// 是撤销派工的最后安全点。默认放行（见 BaseCreatureExt），有门口风险的异想体覆写。
        /// </summary>
        bool ViableAtDoor(AgentModel agent, SkillTypeInfo skill, out string message);

        bool CheckConfidence(AgentModel agent, SkillTypeInfo skill);

        float ConfidenceMultiplifier(AgentModel agent, SkillTypeInfo skill);

        float GetConfidence(AgentModel agent, SkillTypeInfo skill);

        bool TryGetEGOGift(out EquipmentTypeInfo gift);
    }
}