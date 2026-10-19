namespace AutoInority.Creature
{
    internal class WellcheersExt : BaseCreatureExt
    {
        // 四类工作全部安全且回资源：本能/洞察单回 40%/80%，沟通/压迫双回 30%/60%
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Instinct, Insight, Attachment, Repression };

        public WellcheersExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            // 工作结束时异想体心情为差（BAD）→ 员工被按进饮料里溺亡
            if (_creature.feelingState == CreatureFeelingState.BAD)
            {
                message = Message(Angela.Creature.Wellcheers, agent, skill);
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }
    }
}
