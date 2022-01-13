using System.Collections.Generic;

namespace AutoInority
{
    public class CenterBrain
    {
        private static CenterBrain _instance;

        private readonly Stack<Record> records = new Stack<Record>();

        public static CenterBrain Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CenterBrain();
                }
                return _instance;
            }
        }

        internal static Record FindLastRecord(CreatureModel creature)
        {
            foreach (var record in Instance.records)
            {
                if (record.Creature.metaInfo.id == creature.metaInfo.id)
                {
                    return record;
                }
            }
            return null;
        }

        internal static Record FindLastRecord(AgentModel agent)
        {
            foreach (var record in Instance.records)
            {
                if (record.Agent.name == agent.name)
                {
                    return record;
                }
            }
            return null;
        }

        internal static void AddRecord(AgentModel agent, CreatureModel creature, SkillTypeInfo skill)
        {
            if (creature.script is HappyTeddy)
            {
                Instance.records.Push(new Record()
                {
                    Agent = agent,
                    Creature = creature,
                    Skill = skill,
                });
            }
        }

        internal sealed class Record
        {
            public AgentModel Agent { get; set; }

            public CreatureModel Creature { get; set; }

            public SkillTypeInfo Skill { get; set; }
        }
    }
}