using System;
using System.Collections.Generic;
using System.Linq;

using AutoInority.Extentions;

namespace AutoInority
{
    public sealed class Candidate
    {
        public AgentModel Agent { get; private set; }

        public CreatureModel Creature { get; private set; }

        public float Distance { get; private set; }

        public double ExploitConfidence { get; private set; }

        public bool HasAnotherGift { get; private set; }

        public bool HasGift { get; private set; }

        public bool HasReachedExpLimit { get; private set; }

        public double ManageConfidence { get; private set; }

        public SkillTypeInfo Skill { get; private set; }

        public Candidate(AgentModel agent, CreatureModel creature, SkillTypeInfo skill)
            : this(agent, creature, skill, Graph.Distance(agent, creature), 0.0, 0.0)
        {
            HasGift = agent.HasGift(creature, out var gift);
            HasAnotherGift = agent.HasAnotherGift(gift);
            HasReachedExpLimit = agent.HasReachedExpLimit(skill.rwbpType, out _);

            var ext = creature.GetExtension();
            var confidence = ext.GetConfidence(agent, skill);
            ManageConfidence = confidence * (1 - 0.002 * Distance) * ext.ConfidenceMultiplifier(agent, skill);
            ExploitConfidence = confidence * (1 - 0.001 * Distance) * (HasReachedExpLimit ? 0.8 : 1) * (HasGift || HasAnotherGift ? 0.5 : 1) * ext.ConfidenceMultiplifier(agent, skill);
        }

        public static int ExploitComparer(Candidate x, Candidate y) => y.ExploitConfidence.CompareTo(x.ExploitConfidence);

        // 测试构造器：距离与置信度直接注入，绕开 Graph.Distance/GetConfidence 的游戏链
        internal Candidate(AgentModel agent, CreatureModel creature, SkillTypeInfo skill, float distance, double manageConfidence, double exploitConfidence)
        {
            Agent = agent;
            Creature = creature;
            Skill = skill;
            Distance = distance;
            ManageConfidence = manageConfidence;
            ExploitConfidence = exploitConfidence;
            StatLevel = StatLevelOf(agent, skill.rwbpType);
        }

        /// <summary>工作对应的主属性等级（1-5；65+ 为 4 级、85+ 为 5 级，成功率查表跳变点）。</summary>
        public int StatLevel { get; }

        /// <summary>派工置信度的省着用排序：先用低属性等级的（过阈即止），
        /// 同级挑更稳的，再就近——高属性员工留给 WAW/ALEPH 等高门槛目标。</summary>
        public static int SufficientComparer(Candidate x, Candidate y)
        {
            var byLevel = x.StatLevel.CompareTo(y.StatLevel);
            if (byLevel != 0)
            {
                return byLevel;
            }
            var byConfidence = y.ManageConfidence.CompareTo(x.ManageConfidence);
            if (byConfidence != 0)
            {
                return byConfidence;
            }
            return x.Distance.CompareTo(y.Distance);
        }

        // exploit 同构：同级里礼物效率（ExploitConfidence）优先
        public static int ExploitSufficientComparer(Candidate x, Candidate y)
        {
            var byLevel = x.StatLevel.CompareTo(y.StatLevel);
            if (byLevel != 0)
            {
                return byLevel;
            }
            var byExploit = y.ExploitConfidence.CompareTo(x.ExploitConfidence);
            if (byExploit != 0)
            {
                return byExploit;
            }
            return x.Distance.CompareTo(y.Distance);
        }

        /// <summary>工作类型 → 主属性等级（R 勇气/W 谨慎/B 自律/P 正义；N 无属性恒 1 级）。</summary>
        internal static int StatLevelOf(AgentModel agent, RwbpType type)
        {
            switch (type)
            {
                case RwbpType.R:
                    return agent.Rstat;
                case RwbpType.W:
                    return agent.Wstat;
                case RwbpType.B:
                    return agent.Bstat;
                case RwbpType.P:
                    return agent.Pstat;
                default:
                    return 1;
            }
        }

        public static List<Candidate> Suggest(IEnumerable<AgentModel> agents, IEnumerable<CreatureModel> creatures, HashSet<RwbpType> types = null)
        {
            var candidates = new List<Candidate>();
            foreach (var creature in creatures)
            {
                var ext = creature.GetExtension();
                var skillSets = ext.SkillSets.Where(x => types == null || types.Contains(x.rwbpType));

                foreach (var agent in agents)
                {
                    foreach (var skill in skillSets)
                    {
                        var b1 = ext.CanWorkWith(agent, skill, out _);
                        var b2 = ext.CheckConfidence(agent, skill);
                        Log.Debug($"{creature.metaInfo.name}, b1: {b1}, b2: {b2}");
                        if (b1 && b2)
                        {
                            candidates.Add(new Candidate(agent, creature, skill));
                        }
                    }
                }
            }
            return candidates;
        }

        public static List<Candidate> Suggest(AgentModel agent, IEnumerable<CreatureModel> creatures, HashSet<RwbpType> types = null) => Suggest(new[] { agent }, creatures, types);

        public static List<Candidate> Suggest(IEnumerable<AgentModel> agents, CreatureModel creature) => Suggest(agents, new[] { creature });

        public static int TrainComparer(AgentModel x, AgentModel y) => x.TotalStats().CompareTo(y.TotalStats());

        public void Apply()
        {
            try
            {
                var sprite = CommandWindow.CommandWindow.CurrentWindow.GetWorkSprite(Skill.rwbpType);
                Agent.ManageCreature(Creature, Skill, sprite);
                Agent.counterAttackEnabled = false;
                Creature.Unit.room.OnWorkAllocated(Agent);
                Creature.script.OnWorkAllocated(Skill, Agent);
                AngelaConversation.instance.MakeMessage(AngelaMessageState.MANAGE_START, Agent, Skill, Creature);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public bool IsAvailable() => Agent.IsAvailable() && Creature.IsAvailable();

        public override string ToString() => $"{Agent.name} ({ExploitConfidence}, {ManageConfidence}), Distance: {Distance}, HasGift: {HasGift}, HasAnotherGift: {HasAnotherGift}, Exp: {HasReachedExpLimit}";
    }
}