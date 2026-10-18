namespace AutoInority.Creature
{
    using AutoInority.Extentions;

    internal class ShyLook : BaseCreatureExt
    {
        private ShyThing Shy => (ShyThing)Script;

        public ShyLook(CreatureModel creature) : base(creature)
        {
        }

        protected override float CalculateWorkSuccessProb(AgentModel agent, SkillTypeInfo skill)
        {
            return 0.2f;
        }

        protected override float GetDamageMultiplierInWork(AgentModel agent, SkillTypeInfo skill)
        {
            return 2f;
        }

        /// <summary>
        /// 门检：表情 3-5s 随机变换（0-4），派工路上照常变换，员工抵达工作位开工前一瞬复检；
        /// 表情 2/3/4（伤害 1.0x/1.5x/2.0x、成功率 +0/-30/-50）弹回待机。
        /// animScript 在 OnViewInit 前为 null，视为不宜开工。
        /// </summary>
        public override bool ViableAtDoor(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (Shy.animScript == null || ShyLookPolicy.ShouldBounce(Shy.animScript.GetFaceType()))
            {
                var face = Shy.animScript != null ? Shy.animScript.GetFaceType().ToString() : "æªç¥";
                message = string.Format(Angela.CreatureLog.ShyLookBounce, agent.Tag(), _creature.Tag(), face);
                return false;
            }
            message = null;
            return true;
        }
    }
}