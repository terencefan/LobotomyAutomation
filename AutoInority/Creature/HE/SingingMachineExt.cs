using AutoInority.Extentions;

namespace AutoInority.Creature
{
    internal class SingingMachineExt : BaseCreatureExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Instinct, Insight, Repression };

        public SingingMachineExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            // counter 归零后下一名进房者无条件被吃（OnEnterRoom），随后抽取连环攻击者
            if (QliphothCounter <= 0)
            {
                message = Message(Angela.Creature.SingingMachineStarved, agent, skill);
                return false;
            }
            if (agent.Rstat > 3 || agent.Bstat < 3)
            {
                message = string.Format(Angela.Creature.SiningMachine, agent.Tag(), _creature.Tag(), skill.Tag());
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }

        // counter 上限 1：BAD 即归零 → 只接受 GOOD 置信
        public override bool CheckWorkConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            return IsOverloaded || GoodConfidence(agent, skill) > CreatureEscapeConfidence;
        }
    }
}
