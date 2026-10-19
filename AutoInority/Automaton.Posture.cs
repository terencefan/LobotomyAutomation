using System.Collections.Generic;

using AutoInority.Posture;

namespace AutoInority
{
    internal partial class Automaton
    {
        private readonly Dictionary<PostureEnum, PostureBase> _postures = new Dictionary<PostureEnum, PostureBase>
        {
            { PostureEnum.PRODUCTION, new ProductionPosture() },
            { PostureEnum.SUPPRESS, new SuppressPosture() },
        };

        public PostureEnum CurrentPostureEnum { get; private set; }

        internal PostureBase CurrentPosture => _postures[CurrentPostureEnum];

        /// <summary>
        /// 切换调度姿态：唯一切换入口（本期 B 键手动，二期事件评估器同走此处）。
        /// 姿态是 Angela 的姿态（产品定案），切换播报走 Announce 气泡+日志二合一。
        /// </summary>
        public void SwitchPosture(PostureEnum posture)
        {
            if (posture == CurrentPostureEnum)
            {
                return;
            }
            CurrentPosture.OnExit();
            CurrentPostureEnum = posture;
            CurrentPosture.OnEnter();
            Angela.Announce(PostureMessage());
        }

        public void TogglePosture() => SwitchPosture(CurrentPostureEnum == PostureEnum.PRODUCTION ? PostureEnum.SUPPRESS : PostureEnum.PRODUCTION);

        private string PostureMessage() => CurrentPostureEnum == PostureEnum.PRODUCTION ? Angela.Automaton.PostureProduction : Angela.Automaton.PostureSuppress;
    }
}
