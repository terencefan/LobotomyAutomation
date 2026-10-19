namespace AutoInority.Creature
{
    internal class ArmyInBlackExt : ExpectNormalExt
    {
        // 压迫(P)工作完成即扣逆卡巴拉计数（OnSkillGoalComplete），BAD 也扣、
        // 每 5 人次设施内死亡再扣；唯一回充手段是镇压其他异想体。计数归零
        // =本体出逃+已开放部门各撤出一只黑色军团。本能(R) workProb=0 派了白跑，一并排除。
        public override SkillTypeInfo[] SkillSets => new SkillTypeInfo[] { Insight, Attachment };

        public ArmyInBlackExt(CreatureModel creature) : base(creature)
        {
        }
    }
}
