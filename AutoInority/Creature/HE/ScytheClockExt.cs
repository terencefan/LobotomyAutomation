namespace AutoInority.Creature
{
    internal class ScytheClockExt : BaseCreatureExt
    {
        public ScytheClockExt(CreatureModel creature) : base(creature)
        {
        }

        // 无计数器、无属性死亡条件、四类工作皆可；唯一风险=工作结果把心情打到差（BAD）
        // → 玩家手动暂停时随机处刑一名存活员工（KillOnce），计数跨天持久化，
        // XIII 后每 1.75s 杀一人至 25。只接受 GOOD 置信，不给 BAD 留口子
        public override bool CheckWorkConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            return IsOverloaded || GoodConfidence(agent, skill) > CreatureEscapeConfidence;
        }
    }
}
