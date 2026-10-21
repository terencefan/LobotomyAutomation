namespace AutoInority.Creature
{
    internal class DerFreischutzExt : ExpectGoodAndNormalExt
    {
        protected override SkillTypeInfo[] NormalSkillSets { get; } = { Instinct, Insight, Repression };

        protected override SkillTypeInfo[] GoodSkillSets { get; } = { Repression };

        public DerFreischutzExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (agent.justiceLevel < 3)
            {
                message = Message(Angela.Creature.DerFreischutzExt, agent, skill);
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }

        // NORM/BAD 结果都会扣计数（OnReleaseWork）， justice<3 完工还会再扣（L69-74）：
        // 不吃 counter>1 的缓冲，一律只接受 GOOD 置信（基类 Exploit 放宽臂仅 TETH 以下
        // 生效，HE 本就不可达）
        public override bool CheckWorkConfidence(AgentModel agent, SkillTypeInfo skill)
        {
            return IsOverloaded || GoodConfidence(agent, skill) > CreatureEscapeConfidence;
        }
    }
}