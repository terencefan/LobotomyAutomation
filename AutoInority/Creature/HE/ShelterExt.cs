using System;
using System.Linq;

namespace AutoInority.Creature
{
    internal class ShelterExt : ChannelKitExt
    {
        // 进房无敌+每 5s 回 8HP/8SP（Shelter.cs L44-71）；满 30s 后每 5s 随机
        // 清一个异想体计数器（L58-70）——出逃源头。原 CanStop 恒真=秒断零收益。
        // 28s 收手（同 TheresiaExt 留结算边际），或持有者满血满精神提前收工
        private const float StopAt = 28f;

        public ShelterExt(CreatureModel kit) : base(kit)
        {
        }

        // 调用契约：ChannelKitExt.OnFixedUpdate 已保证 currentSkill 非空才问 CanStop
        public override bool CanStop()
        {
            var skill = _kit.currentSkill;
            if (skill.elapsedTime >= StopAt)
            {
                return true;
            }
            var agent = skill.agent;
            return agent.hp >= agent.maxHp && agent.mental >= agent.maxMental;
        }

        // ICU 分诊：只收伤员——未回满即收治（由 CanStop 满状态放出），健康者不占机
        public override bool CanUse(AgentModel agent)
            => agent.hp < agent.maxHp || agent.mental < agent.maxMental;

        // IsAvailable 的 90% 线会把重伤员挡在候选池外，收治必须绕开它：
        // 只取空闲带伤者；重伤绝对优先（三档分诊），同档内就近
        public override AgentModel SelectAgent()
        {
            return AgentManager.instance.GetAgentList()
                .Where(IsPatient)
                .OrderBy(Triage)
                .ThenBy(Distance)
                .FirstOrDefault();
        }

        // 分诊三档：两维中更差的比值 <60% 重伤、<90% 中伤、其余轻伤
        private static int Triage(AgentModel agent)
        {
            var worst = Math.Min(agent.hp / agent.maxHp, agent.mental / agent.maxMental);
            return worst < 0.6f ? 0 : (worst < 0.9f ? 1 : 2);
        }

        // 主动收治：避难所空闲且设施内有人带伤即要人——不等 isOverloaded（那与救人无关）
        public override bool WantsDispatch()
            => _kit.currentSkill == null && AgentManager.instance.GetAgentList().Any(IsPatient);

        private static bool IsPatient(AgentModel agent)
        {
            // 只从空闲池收治：不打断正在工作/镇压的员工，他们闲下来自然轮到
            return agent.GetState() == AgentAIState.IDLE
                && !agent.HasUnitBuf(UnitBufType.DEATH_ANGEL_BETRAYER)
                && (agent.hp < agent.maxHp || agent.mental < agent.maxMental);
        }
    }
}
