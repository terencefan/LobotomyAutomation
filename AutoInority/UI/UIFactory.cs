using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AutoInority.UI
{
    // uGUI 程序化构建的共享 helper（自 ExploitPanel 提取，供控制条等新 UI 复用）。
    // 全 mod/游戏零 IMGUI，新 UI 一律走本工厂：rect/image/text/button 程序化拼装，
    // 颜色取游戏调色板（UIColorManager），字体走 currentLanguageFont 兜底微软雅黑。
    internal static class UIFactory
    {
        private static Font _font;

        private static Sprite _frameSprite;

        // GlobalCanvas tag 虽在日场景可找到，但其唯一游戏引用（TextAppearEffect）是死代码，
        // 挂上去不渲染——首选用 BaseMod GlobalNoticeBox 验证过的跨场景挂点：
        // GlobalGameManager 的 child 0（BaseMod 自家弹窗渲染于此，实际可见）
        internal static Transform FindCanvasTransform()
        {
            var manager = GlobalGameManager.instance;
            if (manager != null && manager.transform.childCount > 0)
            {
                return manager.transform.GetChild(0);
            }
            var tagged = GameObject.FindGameObjectWithTag("GlobalCanvas");
            return tagged != null ? tagged.transform : null;
        }

        internal static Font GetFont()
        {
            if (_font == null)
            {
                var languageFont = GlobalGameManager.currentLanguageFont;
                var data = languageFont != null ? languageFont.GetFont(FontType.CONTEXT) : null;
                _font = (data != null && data.font != null) ? data.font : Font.CreateDynamicFontFromOSFont("Microsoft YaHei", 16);
            }
            return _font;
        }

        // 程序化九宫格：16x16 全白贴图 + 2px border，供 section 线框（Sliced）使用
        internal static Sprite GetFrameSprite()
        {
            if (_frameSprite == null)
            {
                var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
                var pixels = new Color32[16 * 16];
                for (var i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = new Color32(255, 255, 255, 255);
                }
                texture.SetPixels32(pixels);
                texture.Apply();
                _frameSprite = Sprite.Create(texture, new Rect(0f, 0f, 16f, 16f), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(2f, 2f, 2f, 2f));
                _frameSprite.name = "AutoInority.Frame";
            }
            return _frameSprite;
        }

        internal static RectTransform CreateRect(string name, Transform parent)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        internal static Image CreateImage(string name, Transform parent, Color color)
        {
            var rect = CreateRect(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        internal static Text CreateText(string name, Transform parent, string content, int size, Color color, TextAnchor anchor)
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

        // 带标签的实心按钮：底图承接点击（targetGraphic），标签全屏居中。
        // 定位（锚点/位置/尺寸）由调用方对返回按钮的 rectTransform 设置；
        // 改文案用 GetLabel(button)，高亮用 Highlight(button.image, active)。
        internal static Button CreateButton(string name, Transform parent, string label, int labelSize, Color labelColor, UnityAction onClick)
        {
            var rect = CreateRect(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.12f);
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);
            var text = CreateText("Label", rect, label, labelSize, labelColor, TextAnchor.MiddleCenter);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
            return button;
        }

        internal static Text GetLabel(Button button) => button.GetComponentInChildren<Text>();

        // 状态高亮：激活=SefiraGreen（游戏调色板，兜底同 ExploitPanel 指示灯），否则默认暗底
        internal static void Highlight(Button button, bool active)
        {
            var image = button.image;
            if (image == null)
            {
                return;
            }
            var manager = UIColorManager.instance;
            image.color = active ? (manager != null ? manager.Allocate_SefiraGreen : new Color(0.3f, 0.9f, 0.4f)) : new Color(1f, 1f, 1f, 0.12f);
        }

        // 顶部锚定、横向撑满（左右各缩进 inset）、自上而下按 top 排布
        internal static void AnchorTopStretch(RectTransform rect, float top, float height, float inset)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -top);
            rect.sizeDelta = new Vector2(-2f * inset, height);
        }
    }
}
