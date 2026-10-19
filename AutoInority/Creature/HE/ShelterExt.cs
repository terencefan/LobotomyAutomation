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
    }
}
