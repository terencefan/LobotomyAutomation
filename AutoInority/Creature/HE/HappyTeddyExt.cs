using AutoInority.Extentions;

namespace AutoInority.Creature
{
    internal class HappyTeddyExt : BaseCreatureExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Insight, Attachment, Repression };

        public HappyTeddyExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            // 游戏在异想体侧记 lastAgent（上次做它的人再进房即被抱杀，OnEnterRoom）；
            // 员工侧 GetRecentWorkedCreature 语义错位：A→泰迪→X→A 回泰迪必死而放行
            if (Script is HappyTeddy teddy && teddy.lastAgent == agent)
            {
                message = string.Format(Angela.Creature.HappyTeddy, agent.Tag(), _creature.Tag(), skill.Tag());
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }
    }
}