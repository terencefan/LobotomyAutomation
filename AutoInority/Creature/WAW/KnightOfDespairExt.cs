namespace AutoInority.Creature
{
    internal class KnightOfDespairExt : BaseCreatureExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Insight, Attachment };

        public KnightOfDespairExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (IsOverloaded)
            {
                // Randomly pick an agent when overloaded.
                return base.CanWorkWith(agent, skill, out message);
            }
            else if (_creature.script is KnightOfDespair script)
            {
                message = Message(Angela.Creature.KnightOfDespair, agent, skill);
                return script.BlessedWorker == null;
            }
            return base.CanWorkWith(agent, skill, out message);
        }
    }
}
