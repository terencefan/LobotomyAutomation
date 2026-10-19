namespace AutoInority.Creature
{
    using AutoInority.Extentions;

    internal class ShyLook : BaseCreatureExt
    {
        private ShyThing Shy => (ShyThing)Script;

        /// <summary>
        /// 五种表情，值 = 游戏 ShyThingAnim.GetFaceType 的 Animator 参数
        /// （0-4；wiki 记作 Face 1-5，差一偏移）。游戏与 wiki 均无官方表情名，
        /// 成员按效果语义命名。表情 3-5s 随机变换，员工进屋开工瞬间冻结为全程表情。
        /// </summary>
        internal enum ShyFace
        {
            /// <summary>0 大笑：伤害 0.5x、成功率 +30、成功 tick 回血（最友好）。</summary>
            Beaming = 0,

            /// <summary>1 微笑：伤害 1.0x、成功率 +10、成功 tick 回 SP。</summary>
            Cheerful = 1,

            /// <summary>2 平淡：伤害 1.0x、成功率 ±0，无危害加成，按普通放行。</summary>
            Plain = 2,

            /// <summary>3 低落：伤害 1.5x、成功率 -30。</summary>
            Sullen = 3,

            /// <summary>4 崩溃：伤害 2.0x、成功率 -50（最危险）。</summary>
            Distraught = 4,
        }

        /// <summary>表情中文名（播报文案用）；越界值回落数字显示。</summary>
        internal static string FaceName(ShyFace face)
        {
            switch (face)
            {
                case ShyFace.Beaming: return "大笑";
                case ShyFace.Cheerful: return "微笑";
                case ShyFace.Plain: return "平淡";
                case ShyFace.Sullen: return "低落";
                case ShyFace.Distraught: return "崩溃";
                default: return ((int)face).ToString();
            }
        }

        public ShyLook(CreatureModel creature) : base(creature)
        {
        }

        /// <summary>
        /// 派工口径：Plain 基线成功率（游戏 _defaultFaceType：伤害 1.0x、成功率 ±0）。
        /// 基类链经 script.OnBonusWorkProb() 混入当前脸修正（表情 3-5s 随机重掷），
        /// 此处剥除——派工置信度/排序需要与脸无关的稳定口径；门口口径见 ProbAtDoor。
        /// </summary>
        protected override float CalculateWorkSuccessProb(AgentModel agent, SkillTypeInfo skill)
        {
            return base.CalculateWorkSuccessProb(agent, skill) - Shy.OnBonusWorkProb() / 100f;
        }

        /// <summary>
        /// 派工口径：Plain 基线伤害倍率 1.0x，供 CheckSurvive 稳定估算；
        /// 实际伤害由游戏在结算时按冻结脸自行取值（ShyThing.GetDamageMultiplierInWork）。
        /// </summary>
        protected override float GetDamageMultiplierInWork(AgentModel agent, SkillTypeInfo skill)
        {
            return 1f;
        }

        /// <summary>
        /// 门检：表情 3-5s 随机变换，派工路上照常变换，员工抵达工作位开工前一瞬复检；
        /// 平淡及更好（伤害 ≤1.0x、成功率 ≥±0）放行；低落/崩溃（1.5x/2.0x、-30/-50）
        /// 看能力：冻结脸修正后的真实置信度严格大于 CreatureEscapeConfidence 及格线可硬上，
        /// 不过线弹回待机。animScript 在 OnViewInit 前为 null，视为不宜开工。
        /// </summary>
        protected override bool DoorCheck(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            var face = FaceTypeAtDoor();
            if (face == null || (face.Value > ShyFace.Plain && ConfidenceAtDoor(agent, skill) <= CreatureEscapeConfidence))
            {
                message = string.Format(Angela.CreatureLog.ShyLookBounce, agent.Tag(), _creature.Tag(), face == null ? "未知" : FaceName(face.Value));
                return false;
            }
            message = null;
            return true;
        }

        /// <summary>
        /// 门口真实置信度（坏脸硬上的依据）：与派工口径配对——派工剥脸看中性期望，
        /// 门口把冻结脸修正算回来，在概率层重卷积（置信度对 prob 非线性，不得在置信度上加减）。
        /// 与 ExpectNormalExt 等 CheckWorkConfidence 家族同及格线（CreatureEscapeConfidence）。
        /// 虚缝：prob 来源离线不可驱动，测试子类覆写 ProbAtDoor 注入编程值以驱动本体。
        /// </summary>
        protected virtual float ConfidenceAtDoor(AgentModel agent, SkillTypeInfo skill)
        {
            return Confidence.InRange(_creature.MaxCube(), ProbAtDoor(agent, skill), _creature.NormalBound() + 1);
        }

        /// <summary>
        /// 门口真实成功率：基类真实链——经 script.OnBonusWorkProb() 混入冻结脸修正、
        /// 含观察/装备/buf 加成、0.95 封顶（镜像游戏 UseSkill 管道）。
        /// 虚缝：链级联游戏工作概率管道（bare 不可驱动），离线测试以子类覆写注入编程值。
        /// </summary>
        protected virtual float ProbAtDoor(AgentModel agent, SkillTypeInfo skill) => base.CalculateWorkSuccessProb(agent, skill);

        /// <summary>
        /// 门口表情：animScript 未就绪（OnViewInit 前）返回 null，视为不宜开工。
        /// 虚缝：GetFaceType 绑 Unity Animator（引擎 native），离线测试以子类覆写注入编程值。
        /// </summary>
        protected virtual ShyFace? FaceTypeAtDoor()
        {
            var anim = Shy.animScript;
            return anim != null ? (ShyFace?)anim.GetFaceType() : null;
        }
    }
}
