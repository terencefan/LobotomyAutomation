namespace AutoInority.Creature
{
    internal class LadyLookingAtWallExt : ExpectGoodAndNormalExt
    {
        // 沟通(B)进门即扣逆卡巴拉计数（OnEnterRoom skill id==3），唯一排除项
        protected override SkillTypeInfo[] NormalSkillSets { get; } = { Instinct, Insight, Repression };

        public LadyLookingAtWallExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            // 自律<2：工作结束被夺走心智（OnFinishWork temperance<2 → 冻结+恐慌）
            if (agent.temperanceLevel < 2)
            {
                message = Message(Angela.Creature.LadyLookingAtWall, agent, skill);
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }
    }
}