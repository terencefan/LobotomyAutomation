namespace AutoInority.Creature
{
    using AutoInority.Extentions;

    /// <summary>
    /// 今天也很害羞 O-01-92：表情 0-4 随机变换（3-5s 一轮），员工进屋开工瞬间冻结为全程表情。
    /// 伤害倍率 [0.5,1,1,1.5,2]、成功率加成 [+30,+10,0,-30,-50] 均按表情。
    /// 策略：只在表情 0/1 放行派工（实时按表情计算置信度与伤害，基类已接游戏脚本）；
    /// 表情 0 时标记紧急抢窗口；在途表情变坏则撤回。TODO 表情 0/1 成功 tick 的回血
    /// （20% HP+MP / 20% MP）尚未纳入生存模型（Confidence.Survive 无回复概念）。
    /// </summary>
    internal class ShyLook : ExpectNormalExt
    {
        private ShyThing Shy => (ShyThing)Script;

        /// <summary>
        /// 表情 0/1 对工作友好（伤害 0.5x/1.0x、成功率 +30/+10、成功 tick 回血）；2/3/4 不宜开工。
        /// animScript 在 OnViewInit 前为 null，视为不可工作。
        /// </summary>
        private bool GoodFace => Shy.animScript != null && Shy.animScript.GetFaceType() <= 1;

        public ShyLook(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (!GoodFace)
            {
                message = string.Format(Angela.Creature.ShyLook, agent.Tag(), _creature.Tag(), skill.Tag());
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }

        public override bool CanKeepWorking(AgentModel agent) => GoodFace;

        public override bool IsUrgent => GoodFace;
    }
}
