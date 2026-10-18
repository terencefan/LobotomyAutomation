using AutoInority.Extentions;

namespace AutoInority.Creature
{
    internal class OneBadManyGoodExt : BaseCreatureExt
    {
        // TODO（顺水推舟方案，留待日后决定）：白夜（100015）逃逸时，游戏对一罪与百善的
        // 任何工作都不查类型，只要满箱就触发 OnSkillGoalComplete -> OnSuppressedByConfess，
        // 单次满箱即处决白夜（0.3s 死亡计时器，且不再掉落 E.G.O 武器「失乐园」）。
        // 若日后想让自动化主动利用这一点：在白夜逃逸时反向放开并优先派沟通等级最高的职工，
        // 刷满箱自动处决白夜——代价是必然丢失乐园，需产品层面确认后再实现。
        public override SkillTypeInfo[] SkillSets { get; } = new SkillTypeInfo[] { Insight, Attachment };

        public OneBadManyGoodExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            if (IsOverloaded)
            {
                return base.CanWorkWith(agent, skill, out message);
            }
            // 白夜逃逸期间暂停对一罪与百善的自动化派工：此刻常规工作的满箱同样会触发
            // 赎罪处决（游戏月记 bug，不检查工作类型），是否忏悔应由主管亲自决定
            if (WhiteNightEscaped())
            {
                message = string.Format(Angela.Creature.OneSinConfession, agent.Tag(), _creature.Tag(), skill.Tag());
                return false;
            }
            return base.CanWorkWith(agent, skill, out message);
        }

        private static bool WhiteNightEscaped()
        {
            return CreatureManager.instance.FindCreature(100015L)?.IsEscapedOnlyEscape() ?? false;
        }
    }
}
