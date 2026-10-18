using System;
using System.Collections.Generic;
using System.Linq;

using AutoInority.Extentions;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AutoInority.UI
{
    // 种地管理面板：O 键开关。只显示正在 farm 的异想体，点击行即关闭该异想体的种地。
    // 纯叠加 UI，不暂停游戏；刷新与拖动均用 unscaled 时间，暂停/倍速下照常工作。
    // 样式取自游戏调色板（UIColorManager）与游戏字体，不引入外部素材。
    internal class FarmPanel : MonoBehaviour
    {
        private const float GroupHeaderHeight = 30f;

        private const float HintHeight = 24f;

        private const float Padding = 12f;

        private const float PanelWidth = 480f;

        private const float RefreshInterval = 1f;

        private const float RowHeight = 48f;

        private const float TitleBarHeight = 44f;

        private static readonly string[] RiskLevelNames = { "ZAYIN", "TETH", "HE", "WAW", "ALEPH" };

        private static FarmPanel _instance;

        private static Font _font;

        private RectTransform _content;

        private Vector2 _dragStartMouse;

        private Vector2 _dragStartPanel;

        private Text _groupButtonLabel;

        private GroupMode _mode = GroupMode.Department;

        private float _refreshTimer;

        private RectTransform _root;

        internal enum GroupMode
        {
            Department,

            RiskLevel
        }

        internal static void Toggle()
        {
            if (_instance == null)
            {
                Create();
            }
            else
            {
                Destroy(_instance.gameObject);
                Log.Debug("FarmPanel: closed");
            }
        }

        private static void Create()
        {
            if (GlobalGameManager.instance == null)
            {
                Log.Info("FarmPanel: GlobalGameManager not ready, skip");
                return;
            }
            var parent = FindCanvasTransform();
            if (parent == null)
            {
                Log.Info("FarmPanel: no UI canvas found, skip");
                return;
            }
            var root = new GameObject("AutoInority.FarmPanel", typeof(RectTransform), typeof(FarmPanel));
            root.transform.SetParent(parent, false);
            _instance = root.GetComponent<FarmPanel>();
            _instance.BuildRoot();
            _instance.RebuildRows();
            Log.Debug($"FarmPanel: opened under {parent.name}");
        }

        // GlobalCanvas tag 虽在日场景可找到，但其唯一游戏引用（TextAppearEffect）是死代码，
        // 挂上去不渲染——首选用 BaseMod GlobalNoticeBox 验证过的跨场景挂点：
        // GlobalGameManager 的 child 0（BaseMod 自家弹窗渲染于此，实际可见）
        private static Transform FindCanvasTransform()
        {
            var manager = GlobalGameManager.instance;
            if (manager != null && manager.transform.childCount > 0)
            {
                return manager.transform.GetChild(0);
            }
            var tagged = GameObject.FindGameObjectWithTag("GlobalCanvas");
            return tagged != null ? tagged.transform : null;
        }

        private static Font GetFont()
        {
            if (_font == null)
            {
                var languageFont = GlobalGameManager.currentLanguageFont;
                var data = languageFont != null ? languageFont.GetFont(FontType.CONTEXT) : null;
                _font = (data != null && data.font != null) ? data.font : Font.CreateDynamicFontFromOSFont("Microsoft YaHei", 16);
            }
            return _font;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            var rect = CreateRect(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Text CreateText(string name, Transform parent, string content, int size, Color color, TextAnchor anchor)
        {
            var rect = CreateRect(name, parent);
            var text = rect.gameObject.AddComponent<Text>();
            text.text = content;
            text.font = GetFont();
            text.fontSize = size;
            text.color = color;
            text.alignment = anchor;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        // 顶部锚定、横向撑满（左右各缩进 Padding）、自上而下按 top 排布
        private static void AnchorTopStretch(RectTransform rect, float top, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -top);
            rect.sizeDelta = new Vector2(-2f * Padding, height);
        }

        private static Color TextColor(float alpha = 1f)
        {
            var manager = UIColorManager.instance;
            var color = manager != null ? manager.uiTextDefColor : Color.white;
            color.a *= alpha;
            return color;
        }

        private static int RiskLevelOf(CreatureModel creature) => (int)creature.metaInfo.GetRiskLevel();

        private static Color RiskColor(CreatureModel creature)
        {
            var manager = UIColorManager.instance;
            var colors = manager != null ? manager.RiskLevelColor : null;
            var level = RiskLevelOf(creature);
            if (colors != null && level >= 0 && level < colors.Length)
            {
                return colors[level];
            }
            return TextColor();
        }

        private static string CurrentAgentLabel(CreatureModel creature)
        {
            var skill = creature.currentSkill;
            return (skill != null && skill.agent != null) ? skill.agent.name : "—";
        }

        private void BuildRoot()
        {
            _root = GetComponent<RectTransform>();
            _root.anchorMin = Vector2.one;
            _root.anchorMax = Vector2.one;
            _root.pivot = Vector2.one;
            _root.anchoredPosition = new Vector2(-16f, -16f);
            _root.sizeDelta = new Vector2(PanelWidth, TitleBarHeight);

            var background = CreateImage("Background", _root, new Color(0f, 0f, 0f, 0.65f));
            background.rectTransform.anchorMin = Vector2.zero;
            background.rectTransform.anchorMax = Vector2.one;
            background.rectTransform.offsetMin = Vector2.zero;
            background.rectTransform.offsetMax = Vector2.zero;

            BuildTitleBar();

            _content = CreateRect("Content", _root);
            _content.anchorMin = new Vector2(0f, 1f);
            _content.anchorMax = new Vector2(1f, 1f);
            _content.pivot = new Vector2(0.5f, 1f);
            _content.anchoredPosition = new Vector2(0f, -TitleBarHeight);
            _content.sizeDelta = new Vector2(0f, 0f);

            var hint = CreateText("Hint", _root, Angela.FarmPanel.ClickHint, 15, TextColor(0.6f), TextAnchor.MiddleCenter);
            var hintRect = hint.rectTransform;
            hintRect.anchorMin = new Vector2(0f, 0f);
            hintRect.anchorMax = new Vector2(1f, 0f);
            hintRect.pivot = new Vector2(0.5f, 0f);
            hintRect.anchoredPosition = Vector2.zero;
            hintRect.sizeDelta = new Vector2(-2f * Padding, HintHeight);
        }

        private void BuildTitleBar()
        {
            var bar = CreateRect("TitleBar", _root);
            bar.anchorMin = new Vector2(0f, 1f);
            bar.anchorMax = new Vector2(1f, 1f);
            bar.pivot = new Vector2(0.5f, 1f);
            bar.anchoredPosition = Vector2.zero;
            bar.sizeDelta = new Vector2(0f, TitleBarHeight);

            // 标题栏底图保留 raycast：拖动与点击都靠它
            var barImage = bar.gameObject.AddComponent<Image>();
            barImage.color = new Color(1f, 1f, 1f, 0.08f);

            var title = CreateText("Title", bar, Angela.FarmPanel.Title, 22, TextColor(), TextAnchor.MiddleLeft);
            title.rectTransform.anchorMin = Vector2.zero;
            title.rectTransform.anchorMax = Vector2.one;
            title.rectTransform.offsetMin = new Vector2(Padding, 0f);
            title.rectTransform.offsetMax = new Vector2(-180f, 0f);

            var switchRect = CreateRect("SwitchMode", bar);
            switchRect.anchorMin = new Vector2(1f, 0.5f);
            switchRect.anchorMax = new Vector2(1f, 0.5f);
            switchRect.pivot = new Vector2(1f, 0.5f);
            switchRect.sizeDelta = new Vector2(120f, 32f);
            switchRect.anchoredPosition = new Vector2(-48f, 0f);
            var switchImage = switchRect.gameObject.AddComponent<Image>();
            switchImage.color = new Color(1f, 1f, 1f, 0.12f);
            var switchButton = switchRect.gameObject.AddComponent<Button>();
            switchButton.targetGraphic = switchImage;
            switchButton.onClick.AddListener(delegate { HandleGroupSwitch(); });
            _groupButtonLabel = CreateText("Label", switchRect, SwitchModeLabel(), 16, TextColor(), TextAnchor.MiddleCenter);
            _groupButtonLabel.rectTransform.anchorMin = Vector2.zero;
            _groupButtonLabel.rectTransform.anchorMax = Vector2.one;
            _groupButtonLabel.rectTransform.offsetMin = Vector2.zero;
            _groupButtonLabel.rectTransform.offsetMax = Vector2.zero;

            var closeRect = CreateRect("Close", bar);
            closeRect.anchorMin = new Vector2(1f, 0.5f);
            closeRect.anchorMax = new Vector2(1f, 0.5f);
            closeRect.pivot = new Vector2(1f, 0.5f);
            closeRect.sizeDelta = new Vector2(32f, 32f);
            closeRect.anchoredPosition = new Vector2(-8f, 0f);
            var closeImage = closeRect.gameObject.AddComponent<Image>();
            closeImage.color = new Color(1f, 1f, 1f, 0.12f);
            var closeButton = closeRect.gameObject.AddComponent<Button>();
            closeButton.targetGraphic = closeImage;
            closeButton.onClick.AddListener(delegate { Toggle(); });
            var closeLabel = CreateText("Label", closeRect, "×", 18, TextColor(), TextAnchor.MiddleCenter);
            closeLabel.rectTransform.anchorMin = Vector2.zero;
            closeLabel.rectTransform.anchorMax = Vector2.one;
            closeLabel.rectTransform.offsetMin = Vector2.zero;
            closeLabel.rectTransform.offsetMax = Vector2.zero;

            // 拖动：按住标题栏移动面板
            var trigger = bar.gameObject.AddComponent<EventTrigger>();
            var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            down.callback.AddListener(delegate { HandleDragStart(); });
            trigger.triggers.Add(down);
            var drag = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
            drag.callback.AddListener(delegate { HandleDrag(); });
            trigger.triggers.Add(drag);
        }

        private void RebuildRows()
        {
            ClearRows();

            // 快照，防遍历中集合变更
            var creatures = Automaton.Instance.FarmingCreatures.Where(x => x != null && x.IsCreature()).ToList();
            var top = 0f;

            if (creatures.Count == 0)
            {
                var empty = CreateText("Empty", _content, Angela.FarmPanel.Empty, 17, TextColor(0.7f), TextAnchor.MiddleCenter);
                AnchorTopStretch(empty.rectTransform, top, RowHeight);
                top += RowHeight;
            }
            else
            {
                foreach (var group in GroupCreatures(creatures))
                {
                    var header = CreateText("GroupHeader", _content, GroupTitle(group.Key, group.Count()), 16, TextColor(0.85f), TextAnchor.MiddleLeft);
                    AnchorTopStretch(header.rectTransform, top, GroupHeaderHeight);
                    top += GroupHeaderHeight;

                    foreach (var creature in group)
                    {
                        BuildCreatureRow(creature, top);
                        top += RowHeight;
                    }
                }
            }

            _content.sizeDelta = new Vector2(0f, top);
            _root.sizeDelta = new Vector2(PanelWidth, TitleBarHeight + top + HintHeight + Padding);
        }

        private void ClearRows()
        {
            for (var i = _content.childCount - 1; i >= 0; i--)
            {
                Destroy(_content.GetChild(i).gameObject);
            }
        }

        private IEnumerable<IGrouping<int, CreatureModel>> GroupCreatures(List<CreatureModel> creatures)
        {
            if (_mode == GroupMode.Department)
            {
                var ordered = creatures.OrderBy(c => (int)c.sefira.sefiraEnum).ThenBy(c => RiskLevelOf(c)).ThenBy(c => c.metaInfo.name);
                return ordered.GroupBy(c => (int)c.sefira.sefiraEnum);
            }
            var orderedByLevel = creatures.OrderBy(c => RiskLevelOf(c)).ThenBy(c => (int)c.sefira.sefiraEnum).ThenBy(c => c.metaInfo.name);
            return orderedByLevel.GroupBy(c => RiskLevelOf(c));
        }

        private string GroupTitle(int key, int count)
        {
            string name;
            if (_mode == GroupMode.Department)
            {
                var sefiraName = SefiraName.GetLocalizingSefiraName((SefiraEnum)key);
                name = string.IsNullOrEmpty(sefiraName) ? ((SefiraEnum)key).ToString() : sefiraName;
            }
            else
            {
                name = (key >= 0 && key < RiskLevelNames.Length) ? RiskLevelNames[key] : ((RiskLevel)key).ToString();
            }
            return $"{name} ×{count}";
        }

        private void BuildCreatureRow(CreatureModel creature, float top)
        {
            var row = CreateRect("Row", _content);
            AnchorTopStretch(row, top, RowHeight - 2f);

            var image = row.gameObject.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.06f);

            var button = row.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(delegate { HandleRowClick(creature); });

            var portrait = CreateImage("Portrait", row, Color.white);
            var sprite = Add_On.GetPortrait(creature.metaInfo.portraitSrc);
            if (sprite == null)
            {
                portrait.enabled = false;
            }
            else
            {
                portrait.sprite = sprite;
                portrait.preserveAspect = true;
            }
            portrait.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            portrait.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            portrait.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            portrait.rectTransform.sizeDelta = new Vector2(36f, 36f);
            portrait.rectTransform.anchoredPosition = new Vector2(Padding + 18f, 0f);

            var name = CreateText("Name", row, creature.metaInfo.name, 18, RiskColor(creature), TextAnchor.MiddleLeft);
            name.rectTransform.anchorMin = new Vector2(0f, 0f);
            name.rectTransform.anchorMax = new Vector2(1f, 1f);
            name.rectTransform.offsetMin = new Vector2(64f, 0f);
            name.rectTransform.offsetMax = new Vector2(-200f, 0f);

            var agentLabel = CreateText("Agent", row, CurrentAgentLabel(creature), 15, TextColor(0.7f), TextAnchor.MiddleRight);
            agentLabel.rectTransform.anchorMin = new Vector2(1f, 0f);
            agentLabel.rectTransform.anchorMax = new Vector2(1f, 1f);
            agentLabel.rectTransform.pivot = new Vector2(1f, 0.5f);
            agentLabel.rectTransform.offsetMin = new Vector2(-196f, 0f);
            agentLabel.rectTransform.offsetMax = new Vector2(-8f, 0f);
        }

        private void HandleRowClick(CreatureModel creature)
        {
            try
            {
                // 只关不放：行仅在面板打开期间存在，若期间该异想体已被移出 farm 集合则忽略
                if (creature != null && creature.Unit != null && creature.Unit.room != null && Automaton.Instance.FarmingCreatures.Contains(creature))
                {
                    Automaton.Instance.ToggleFarming(creature);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            RebuildRows();
        }

        private void HandleGroupSwitch()
        {
            _mode = _mode == GroupMode.Department ? GroupMode.RiskLevel : GroupMode.Department;
            _groupButtonLabel.text = SwitchModeLabel();
            RebuildRows();
        }

        private string SwitchModeLabel() => _mode == GroupMode.Department ? Angela.FarmPanel.ByRiskLevel : Angela.FarmPanel.ByDepartment;

        private void HandleDragStart()
        {
            _dragStartMouse = Input.mousePosition;
            _dragStartPanel = _root.anchoredPosition;
        }

        private void HandleDrag()
        {
            var canvas = _root.GetComponentInParent<Canvas>();
            var scale = canvas != null ? Mathf.Max(canvas.scaleFactor, 0.0001f) : 1f;
            _root.anchoredPosition = _dragStartPanel + ((Vector2)Input.mousePosition - _dragStartMouse) / scale;
        }

        private void Update()
        {
            _refreshTimer += Time.unscaledDeltaTime;
            if (_refreshTimer < RefreshInterval)
            {
                return;
            }
            _refreshTimer = 0f;
            RebuildRows();
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
