using System.Collections.Generic;

namespace AutoInority
{
    public class CenterBrain
    {
        private static CenterBrain _instance;

        private readonly HashSet<AgentModel> blessed = new HashSet<AgentModel>();

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

        internal static int BlessedCount => Instance.blessed.Count;

        internal static void AddRecord(AgentModel agent, CreatureModel creature, SkillTypeInfo skill)
        {
            if (creature.script is HappyTeddy || creature.script is YoungPrince || creature.script is BeautyBeast)
            {
                Instance.records.Push(new Record()
                {
                    Agent = agent,
                    Creature = creature,
                    Skill = skill,
                });
            }
            else if (creature.script is GalaxyBoy)
            {
                Instance.blessed.Add(agent);
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

        internal static List<Record> FindLastRecords(CreatureModel creature, int count)
        {
            var result = new List<Record>();
            foreach (var record in Instance.records)
            {
                if (record.Creature.metaInfo.id == creature.metaInfo.id)
                {
                    result.Add(record);
                }
                if (result.Count == count)
                {
                    return result;
                }
            }
            Log.Info($"Found {result.Count} records for {creature.metaInfo.name}");
            return result;
        }

        internal static List<Record> FindLastRecords(AgentModel agent, int count)
        {
            var result = new List<Record>();
            foreach (var record in Instance.records)
            {
                if (record.Agent.name == agent.name)
                {
                    result.Add(record);
                }
                if (result.Count == count)
                {
                    return result;
                }
            }
            Log.Info($"Found {result.Count} records for {agent.name}");
            return result;
        }

        internal static void Reset()
        {
            _instance = null;
        }

        internal sealed class Record
        {
            public AgentModel Agent { get; set; }

            public CreatureModel Creature { get; set; }

            public SkillTypeInfo Skill { get; set; }
        }
    }
}