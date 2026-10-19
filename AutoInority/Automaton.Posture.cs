using System.Collections.Generic;

using AutoInority.Posture;

namespace AutoInority
{
    internal partial class Automaton
    {
        private readonly Dictionary<PostureEnum, PostureBase> _postures = new Dictionary<PostureEnum, PostureBase>
        {
            { PostureEnum.PRODUCTION, new ProductionPosture() },
            { PostureEnum.TRAINING, new TrainingPosture() },
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

        public void TogglePosture()
        {
            var next = (PostureEnum)(((int)CurrentPostureEnum + 1) % _postures.Count);
            SwitchPosture(next);
        }

        private string PostureMessage()
        {
            switch (CurrentPostureEnum)
            {
                case PostureEnum.PRODUCTION:
                    return Angela.Automaton.PostureProduction;
                case PostureEnum.TRAINING:
                    return Angela.Automaton.PostureTraining;
                default:
                    return Angela.Automaton.PostureSuppress;
            }
        }
    }
}
