using System.Collections.Generic;

namespace AutoInority.Creature
{
    public interface ICreatureExtension
    {
        bool AutoSuppress { get; }

        /// <summary>
        /// 镇压扩展目标：本体之外需一并镇压的子实体（如蕾蒂希娅的女巫之子，须过滤
        /// destroied——GetAliveChilds 不筛死活，尸体仍报 ESCAPE 会让命令永不完结）。
        /// 默认无。
        /// </summary>
        IEnumerable<CreatureModel> SuppressionTargets { get; }

        bool IsUrgent { get; }

        /// <summary>
        /// 是否可接待带心（蕾蒂希娅 LITTLEWITCH_HEART）员工：心未消期间员工只能为她工作，
        /// 默认拒绝，由心之归属地覆写放行（LaetitiaExt）。派工闸（CanWorkWith）与
        /// 门口兜底（ViableAtDoor）均读此值。
        /// </summary>
        bool AcceptsHeartAgent { get; }

        SkillTypeInfo[] SkillSets { get; }

        bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message);

        /// <summary>
        /// 门检：员工抵达工作位、游戏即将创建 UseSkill（UseSkill.InitUseSkillAction）的前一瞬调用，
        /// 返回 false 则拒绝开工并把员工弹回待机通道——此时表情等易变状态刚被定格，
        /// 是撤销派工的最后安全点。总入口（非虚）先过心之归属地兜底，creature 门检覆写
        /// BaseCreatureExt.DoorCheck，默认放行。
        /// </summary>
        bool ViableAtDoor(AgentModel agent, SkillTypeInfo skill, out string message);

        bool CheckConfidence(AgentModel agent, SkillTypeInfo skill);

        float ConfidenceMultiplifier(AgentModel agent, SkillTypeInfo skill);

        float GetConfidence(AgentModel agent, SkillTypeInfo skill);

        bool TryGetEGOGift(out EquipmentTypeInfo gift);
    }
}