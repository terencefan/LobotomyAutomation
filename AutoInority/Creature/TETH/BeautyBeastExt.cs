namespace AutoInority.Creature
{
    public class BeautyBeastExt : BaseCreatureExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Insight, Repression };

        public BeautyBeastExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (skill.rwbpType == RwbpType.P)
            {
                var record = CenterBrain.FindLastRecord(_creature);
                if (record != null && record.Skill.rwbpType == RwbpType.P)
                {
                    message = "Foo";
                    return false;
                }
            }
            return base.CanWorkWith(agent, skill, out message);
        }
    }
}