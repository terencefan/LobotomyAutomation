using System.Collections.Generic;
using System.Linq;

using AutoInority.Extentions;

namespace AutoInority.Creature
{
    internal class LaetitiaExt : BaseCreatureExt
    {
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Instinct, Insight, Attachment };

        // 心之归属地：带 LITTLEWITCH_HEART 的员工只能为她工作
        public override bool AcceptsHeartAgent => true;

        // 女巫之子：GetAliveChilds 不筛死活（CreatureModel.cs:1375），destroied 的
        // 尸体仍报 ESCAPE，不过滤会让镇压命令永不完结刷屏；子实体镇压对本体零
        // 影响（OnChildSuppressed 默认空），纳入只为清场
        public override IEnumerable<CreatureModel> SuppressionTargets =>
            _creature.GetAliveChilds().Where(x => !x.destroied).Select(x => (CreatureModel)x);

        public LaetitiaExt(CreatureModel creature) : base(creature)
        {
        }

        /// <summary>
        /// 心之引信主动回送判定：带 LITTLEWITCH_HEART 的员工处于 MANAGE 且目标非女巫
        /// ——与 LittleWitchBuf.cs:87 的引信点燃条件同款，5-10s 引信步行途中即可引爆，
        /// 门口兜底要到门才拦。调用方对 true 立即 ForcelyCancelWork
        /// （canCancelCurrentWork 守卫，原生清理并回 Sefira 待机点），女巫可用时由
        /// 常规派工接回。
        /// </summary>
        public static bool ShouldRecall(AgentModel agent, CreatureModel witch)
        {
            return witch != null
                && agent.HasUnitBuf(UnitBufType.LITTLEWITCH_HEART)
                && agent.GetState() == AgentAIState.MANAGE
                && agent.target != witch;
        }

        public static void RecallHeartAgents(IList<AgentModel> agents, CreatureModel witch)
        {
            foreach (var agent in agents)
            {
                if (ShouldRecall(agent, witch) && agent.canCancelCurrentWork)
                {
                    var target = agent.target == null ? "误派工作" : agent.target.Tag();
                    Angela.Log($"已把{agent.Tag()}从{target}召回（心之引信）");
                    agent.ForcelyCancelWork();
                }
            }
        }
    }
}
