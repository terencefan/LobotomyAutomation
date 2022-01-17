using System.Collections.Generic;
using System.Linq;

namespace AutoInority.Extentions
{
    public static class SefiraExtensions
    {
        private static readonly Dictionary<SefiraEnum, List<SefiraEnum>> _neibours = new Dictionary<SefiraEnum, List<SefiraEnum>>()
        {
            { SefiraEnum.MALKUT, new List<SefiraEnum>(){SefiraEnum.YESOD, SefiraEnum.NETZACH, SefiraEnum.HOD} },
            { SefiraEnum.YESOD, new List<SefiraEnum>(){SefiraEnum.MALKUT, SefiraEnum.NETZACH, SefiraEnum.HOD} },
            { SefiraEnum.NETZACH, new List<SefiraEnum>(){SefiraEnum.YESOD, SefiraEnum.MALKUT} },
            { SefiraEnum.HOD, new List<SefiraEnum>(){SefiraEnum.YESOD, SefiraEnum.MALKUT} },
            { SefiraEnum.TIPERERTH1, new List<SefiraEnum>(){SefiraEnum.GEBURAH, SefiraEnum.CHESED} },
            { SefiraEnum.TIPERERTH2, new List<SefiraEnum>(){SefiraEnum.GEBURAH, SefiraEnum.CHESED} },
            { SefiraEnum.GEBURAH, new List<SefiraEnum>(){SefiraEnum.TIPERERTH1, SefiraEnum.TIPERERTH2, SefiraEnum.BINAH} },
            { SefiraEnum.CHESED, new List<SefiraEnum>(){SefiraEnum.TIPERERTH1, SefiraEnum.TIPERERTH2, SefiraEnum.CHOKHMAH} },
            { SefiraEnum.CHOKHMAH, new List<SefiraEnum>(){SefiraEnum.CHESED} },
            { SefiraEnum.BINAH, new List<SefiraEnum>(){SefiraEnum.GEBURAH} },
        };

        public static string[] AgentNames(this Sefira sefira) => sefira.agentList.Select(x => x.name).ToArray();

        public static string[] CreatureNames(this Sefira sefira) => sefira.creatureList.Select(x => x.metaInfo.name).ToArray();

        public static IEnumerable<CreatureModel> UrgentCreatures(this Sefira sefira)
        {
            return sefira.creatureList.Where(x => !x.IsKit() && x.IsAvailable() && x.IsUrgent());
        }

        public static IEnumerable<CreatureModel> Creatures(this Sefira sefira)
        {
            return sefira.creatureList.Where(x => !x.IsKit() && x.IsAvailable());
        }

        public static IEnumerable<AgentModel> AvailableAgents(this Sefira sefira)
        {
            return sefira.agentList.Where(x => x.IsAvailable());
        }

        public static IEnumerable<AgentModel> NeibourAgents(this Sefira sefira)
        {
            var r = new List<AgentModel>();
            if (_neibours.TryGetValue(sefira.sefiraEnum, out var neibours))
            {
                foreach (var id in neibours)
                {
                    var neibour = SefiraManager.instance.GetSefira(id);
                    if (neibour != null)
                    {
                        r.AddRange(neibour.AvailableAgents());
                    }
                }
            }
            return r;
        }

        public static float GetPriority(this Sefira sefira)
        {
            switch ((SefiraEnum)sefira.index)
            {
                case SefiraEnum.TIPERERTH1:
                case SefiraEnum.TIPERERTH2:
                    return 1f;
                case SefiraEnum.CHOKHMAH:
                case SefiraEnum.BINAH:
                    return 2f;
                case SefiraEnum.CHESED:
                case SefiraEnum.GEBURAH:
                    return 3f;
                case SefiraEnum.NETZACH:
                case SefiraEnum.HOD:
                    return 4f;
                case SefiraEnum.MALKUT:
                case SefiraEnum.YESOD:
                default:
                    return 5f;
            }
        }

        public static void MoveToNeighborPassage(this Sefira sefira)
        {
        }
    }
}