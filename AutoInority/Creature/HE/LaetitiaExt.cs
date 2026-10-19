namespace AutoInority.Creature
{
    internal class LaetitiaExt : BaseCreatureExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Instinct, Insight, Attachment };

        // 心之归属地：带 LITTLEWITCH_HEART 的员工只能为她工作
        public override bool AcceptsHeartAgent => true;

        public LaetitiaExt(CreatureModel creature) : base(creature)
        {
        }
    }
}