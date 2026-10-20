using System.Reflection;

using AutoInority.Posture;
using AutoInority.UI;

using UnityEngine;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 控制条文案映射层：PostureLabel 三态映射 + 自动化开/关随语言解析。
    /// UI 本体（uGUI 构建链）是引擎调用离线不可驱动，覆盖率门禁黑名单兜住
    /// （ci/filter-coverage.py UI/ 前缀），此处只钉纯映射函数。
    /// </summary>
    [Collection("GameBridge")]
    public class ControlBarTest
    {
        private static void InjectLanguage(SystemLanguage language)
        {
            var globalGameManager = GameBridge.Bare<GlobalGameManager>();
            typeof(GlobalGameManager)
                .GetField("_instance", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .SetValue(null, globalGameManager);
            GameBridge.Set(globalGameManager, "_language", language);
        }

        private static void ClearLanguage()
        {
            typeof(GlobalGameManager)
                .GetField("_instance", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .SetValue(null, null);
        }

        [Fact]
        public void PostureLabelMapsAllThreeStates()
        {
            InjectLanguage(SystemLanguage.ChineseSimplified);
            try
            {
                Assert.Equal("生产", ControlBar.PostureLabel(PostureEnum.PRODUCTION));
                Assert.Equal("训练", ControlBar.PostureLabel(PostureEnum.TRAINING));
                Assert.Equal("镇压", ControlBar.PostureLabel(PostureEnum.SUPPRESS));
            }
            finally
            {
                ClearLanguage();
            }
        }

        [Fact]
        public void AutomationLabelFollowsRunningState()
        {
            InjectLanguage(SystemLanguage.ChineseSimplified);
            try
            {
                Assert.Equal("自动化：开", Angela.ControlBar.AutomationOn);
                Assert.Equal("自动化：关", Angela.ControlBar.AutomationOff);
            }
            finally
            {
                ClearLanguage();
            }
        }
    }
}
