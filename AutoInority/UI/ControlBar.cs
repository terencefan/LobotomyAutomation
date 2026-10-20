using System.Collections.Generic;

using AutoInority.Posture;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AutoInority.UI
{
    // 自动化控制条：常驻底部小工具条，三条入口等价于快捷键（快捷键全部保留）——
    // 自动化总开关（=P）、三姿态直跳（=B 循环的直选版）、异想体总览（=O）。
    // 纯叠加 UI，不暂停游戏；拖动与刷新均用引擎事件（unscaled 下照常工作）。
    // 状态可视：当前姿态/总开关开启/总览开启都以 SefiraGreen 底色高亮，
    // Update 变更检测（缓存三态，变了才刷），无定时重建。
    internal class ControlBar : MonoBehaviour
    {
        private const float AutomationWidth = 132f;

        private const float BarHeight = 40f;

        private const float ButtonGap = 6f;

        private const float ButtonWidth = 84f;

        private const float Padding = 16f;

        private static readonly PostureEnum[] Postures = { PostureEnum.PRODUCTION, PostureEnum.TRAINING, PostureEnum.SUPPRESS };

        private static ControlBar _instance;

        private readonly Dictionary<PostureEnum, Button> _postureButtons = new Dictionary<PostureEnum, Button>();

        private Button _automationButton;

        private Vector2 _dragStartBar;

        private Vector2 _dragStartMouse;

        private Button _overviewButton;

        private RectTransform _root;

        private bool _lastOverviewOpen;

        private PostureEnum _lastPosture = (PostureEnum)(-1);

        private bool _lastRunning = true;

        internal static void EnsureCreated()
        {
            if (_instance != null || GlobalGameManager.instance == null)
            {
                return;
            }
            var parent = UIFactory.FindCanvasTransform();
            if (parent == null)
            {
                return;
            }
            var root = new GameObject("AutoInority.ControlBar", typeof(RectTransform), typeof(ControlBar));
            root.transform.SetParent(parent, false);
            _instance = root.GetComponent<ControlBar>();
            _instance.BuildRoot();
        }

        // 离线可测的文案映射层：姿态短标签与总开关文案随当前语言解析
        internal static string PostureLabel(PostureEnum posture)
        {
            switch (posture)
            {
                case PostureEnum.PRODUCTION:
                    return Angela.ControlBar.PostureProduction;
                case PostureEnum.TRAINING:
                    return Angela.ControlBar.PostureTraining;
                default:
                    return Angela.ControlBar.PostureSuppress;
            }
        }

        private void BuildRoot()
        {
            _root = GetComponent<RectTransform>();
            _root.anchorMin = Vector2.zero;
            _root.anchorMax = Vector2.zero;
            _root.pivot = new Vector2(0f, 0f);
            _root.anchoredPosition = new Vector2(Padding, Padding);
            _root.sizeDelta = new Vector2(AutomationWidth + ButtonGap + 3 * (ButtonWidth + ButtonGap), BarHeight);

            // 整条底图：半透明黑，保留 raycast 承接拖拽（按钮子物体各自处理点击）
            var background = _root.gameObject.AddComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.65f);
            var trigger = _root.gameObject.AddComponent<EventTrigger>();
            var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            down.callback.AddListener(delegate { HandleDragStart(); });
            trigger.triggers.Add(down);
            var drag = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
            drag.callback.AddListener(delegate { HandleDrag(); });
            trigger.triggers.Add(drag);

            var left = Padding / 2f;
            _automationButton = BuildButton("Automation", left, AutomationWidth, Angela.ControlBar.AutomationOn, delegate
            {
                Automaton.Instance.ToggleAutomation();
            });
            left += AutomationWidth + ButtonGap;

            foreach (var posture in Postures)
            {
                var target = posture; // 闭包捕获副本
                _postureButtons[posture] = BuildButton(posture.ToString(), left, ButtonWidth, PostureLabel(posture), delegate
                {
                    Automaton.Instance.SwitchPosture(target);
                });
                left += ButtonWidth + ButtonGap;
            }

            _overviewButton = BuildButton("Overview", left, ButtonWidth, Angela.ControlBar.Overview, delegate
            {
                ExploitPanel.Toggle();
            });

            Refresh();
        }

        private Button BuildButton(string name, float left, float width, string label, UnityEngine.Events.UnityAction onClick)
        {
            var button = UIFactory.CreateButton(name, _root, label, 16, TextColor(), onClick);
            var rect = button.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = new Vector2(width, BarHeight - 8f);
            rect.anchoredPosition = new Vector2(left, 0f);
            return button;
        }

        private static Color TextColor()
        {
            var manager = UIColorManager.instance;
            return manager != null ? manager.uiTextDefColor : Color.white;
        }

        private void Refresh()
        {
            var automaton = Automaton.Instance;
            var running = automaton.IsRunning;
            var posture = automaton.CurrentPostureEnum;
            var overviewOpen = ExploitPanel.IsOpen;

            UIFactory.GetLabel(_automationButton).text = running ? Angela.ControlBar.AutomationOn : Angela.ControlBar.AutomationOff;
            UIFactory.Highlight(_automationButton, running);
            foreach (var pair in _postureButtons)
            {
                UIFactory.Highlight(pair.Value, pair.Key == posture);
            }
            UIFactory.Highlight(_overviewButton, overviewOpen);

            _lastRunning = running;
            _lastPosture = posture;
            _lastOverviewOpen = overviewOpen;
        }

        private void Update()
        {
            var automaton = Automaton.Instance;
            if (automaton.IsRunning == _lastRunning && automaton.CurrentPostureEnum == _lastPosture && ExploitPanel.IsOpen == _lastOverviewOpen)
            {
                return;
            }
            Refresh();
        }

        private void HandleDragStart()
        {
            _dragStartMouse = Input.mousePosition;
            _dragStartBar = _root.anchoredPosition;
        }

        private void HandleDrag()
        {
            var canvas = _root.GetComponentInParent<Canvas>();
            var scale = canvas != null ? Mathf.Max(canvas.scaleFactor, 0.0001f) : 1f;
            _root.anchoredPosition = _dragStartBar + ((Vector2)Input.mousePosition - _dragStartMouse) / scale;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
