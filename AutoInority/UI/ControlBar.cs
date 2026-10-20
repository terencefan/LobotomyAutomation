using System.Collections.Generic;

using AutoInority.Posture;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AutoInority.UI
{
    // 自动化控制条：仅 day 工作时间内显示（部署页/结算页自动隐藏），默认底部居中、
    // 悬于游戏底部命令条上方——BulletWindow 子弹槽条（底边≈屏幕高 23%、顶边≈27%）与
    // RabbitProtocolWindow 兔子按钮（顶边≈28%）所占的条带，锚点 y=0.30 让在其上方。
    // 三条入口等价于快捷键（快捷键全部保留）——自动化总开关（=P）、三姿态直跳
    // （=B 循环的直选版）、异想体总览（=O）。纯叠加 UI，不暂停游戏；
    // 拖动与刷新均用引擎事件（unscaled 下照常工作）。
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

        private GameObject _bar;

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
            _root.anchorMin = new Vector2(0.5f, 0.3f);
            _root.anchorMax = new Vector2(0.5f, 0.3f);
            _root.pivot = new Vector2(0.5f, 0f);
            _root.anchoredPosition = Vector2.zero;

            // 视觉内容整体挂在 Bar 子物体：根物体保持 Update 存活，隐藏时只关 Bar
            var barRect = UIFactory.CreateRect("Bar", _root);
            barRect.anchorMin = Vector2.zero;
            barRect.anchorMax = Vector2.one;
            barRect.offsetMin = Vector2.zero;
            barRect.offsetMax = Vector2.zero;
            _bar = barRect.gameObject;

            // 整条底图：半透明黑，保留 raycast 承接拖拽（按钮子物体各自处理点击）。
            // 原生母题（见 skill ui.md「白天 HUD 布局」）：BulletWindow 命令条 = 半透明黑底
            // + 左右端竖饰线 + 略出血的线框，这里用游戏文字色复刻，绿色只留给状态高亮。
            var accent = TextColor();
            var background = _bar.AddComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.65f);
            var trigger = _bar.AddComponent<EventTrigger>();
            var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            down.callback.AddListener(delegate { HandleDragStart(); });
            trigger.triggers.Add(down);
            var drag = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
            drag.callback.AddListener(delegate { HandleDrag(); });
            trigger.triggers.Add(drag);

            var left = Padding / 2f;
            _automationButton = BuildButton("Automation", _bar.transform, left, AutomationWidth, Angela.ControlBar.AutomationOn, delegate
            {
                Automaton.Instance.ToggleAutomation();
            });
            left += AutomationWidth + ButtonGap;

            foreach (var posture in Postures)
            {
                var target = posture; // 闭包捕获副本
                _postureButtons[posture] = BuildButton(posture.ToString(), _bar.transform, left, ButtonWidth, PostureLabel(posture), delegate
                {
                    Automaton.Instance.SwitchPosture(target);
                });
                left += ButtonWidth + ButtonGap;
            }

            _overviewButton = BuildButton("Overview", _bar.transform, left, ButtonWidth, Angela.ControlBar.Overview, delegate
            {
                ExploitPanel.Toggle();
            });
            left += ButtonWidth;

            // 底图必须包住全部按钮：宽度按布局终值推（右缘 + 右内边距），与左内边距对称
            var width = left + Padding / 2f;
            _root.sizeDelta = new Vector2(width, BarHeight);

            // 左右端竖饰线（BulletWindow LeftLine/RightLine 同款）
            BuildAccentLine("LeftLine", 4f, accent);
            BuildAccentLine("RightLine", width - 4f, accent);

            // 出血外框：程序化九宫格线框（ExploitPanel section 同款），
            // 贴游戏 Frame 比 Pivot 大一圈的惯例
            var frame = UIFactory.CreateRect("Frame", _bar.transform);
            frame.anchorMin = Vector2.zero;
            frame.anchorMax = Vector2.one;
            frame.pivot = new Vector2(0.5f, 0.5f);
            frame.anchoredPosition = Vector2.zero;
            frame.sizeDelta = new Vector2(6f, 6f);
            var frameImage = frame.gameObject.AddComponent<Image>();
            frameImage.sprite = UIFactory.GetFrameSprite();
            frameImage.type = Image.Type.Sliced;
            frameImage.color = new Color(accent.r, accent.g, accent.b, 0.30f);
            frameImage.raycastTarget = false;

            Refresh();
        }

        private void BuildAccentLine(string name, float centerX, Color accent)
        {
            var line = UIFactory.CreateRect(name, _bar.transform);
            line.anchorMin = new Vector2(0f, 0.5f);
            line.anchorMax = new Vector2(0f, 0.5f);
            line.pivot = new Vector2(0.5f, 0.5f);
            line.sizeDelta = new Vector2(3f, BarHeight - 8f);
            line.anchoredPosition = new Vector2(centerX, 0f);
            var image = line.gameObject.AddComponent<Image>();
            image.color = new Color(accent.r, accent.g, accent.b, 0.55f);
            image.raycastTarget = false;
        }

        private Button BuildButton(string name, Transform parent, float left, float width, string label, UnityEngine.Events.UnityAction onClick)
        {
            var button = UIFactory.CreateButton(name, parent, label, 16, TextColor(), onClick);
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
            UpdateVisibility();

            var automaton = Automaton.Instance;
            if (automaton.IsRunning == _lastRunning && automaton.CurrentPostureEnum == _lastPosture && ExploitPanel.IsOpen == _lastOverviewOpen)
            {
                return;
            }
            Refresh();
        }

        // 仅 day 工作时间显示：ManageStarted 在 StartGame 置 true，部署页/结算页均为 false；
        // 跨场景后 currentGameManager 是假 null（Unity 对已销毁对象 == null 为真），一并隐藏。
        // 根物体不能 SetActive(false)，否则 Update 停转再也回不来，只关 Bar 子物体。
        private void UpdateVisibility()
        {
            var manager = GameManager.currentGameManager;
            var visible = manager != null && manager.ManageStarted;
            if (_bar.activeSelf != visible)
            {
                _bar.SetActive(visible);
            }
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
