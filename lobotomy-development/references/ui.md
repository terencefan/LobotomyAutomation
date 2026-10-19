# 游戏内 UI（UGUI 纯代码构建）

AutoInority 的游戏内 UI 一律纯代码构建，无预制体、无外部素材。首例实现：`AutoInority/UI/FarmPanel.cs`（PR #86，O 键轮班管理面板）——做新 UI 先读它。

## 设计语言（定稿，2026-10-19）

**原则：风格一致性 = 颜色全部取自游戏调色板 `UIColorManager` + 游戏字体链路 + 扁平深色外框。** 不引入贴图/圆角/阴影。

| 元素 | 取值 |
|---|---|
| 面板底 | `new Color(0, 0, 0, 0.65f)`（游戏深色弹窗 GlobalNoticeBox 用黑 0.5 遮罩，面板稍深保对比） |
| 标题栏底 | `new Color(1, 1, 1, 0.08f)`；**保留 raycastTarget=true**（拖动/点击靠它） |
| 行底 | `new Color(1, 1, 1, 0.06f)`；按钮图形同色（`button.targetGraphic = image`，默认 ColorTint 过渡即有 hover 反馈） |
| 按钮/次级底 | `new Color(1, 1, 1, 0.12f)` |
| 正文文字 | `UIColorManager.instance.uiTextDefColor`（游戏默认 UI 文字色；instance 可能 null，兜底 `Color.white`） |
| 次级文字 | 文字色 × alpha 0.6~0.85 |
| 强调（风险等级） | `UIColorManager.instance.RiskLevelColor[(int)metaInfo.GetRiskLevel()]` |
| 组 section 线框 | 程序化九宫格（16×16 白图 + `Sprite.Create` border(2,2,2,2)）+ `Image.type=Sliced, fillCenter=true`，着组色 alpha≈0.10；组色：部门 `UIColorManager.GetSefiraColor(sefiraEnum).imageColor`（另有 textColor/ui_1/ui_2），等级即 RiskLevelColor；组标题分隔线 = 组色 alpha 0.55 细条 |
| 开发开关（行内 toggle） | 100×36 Button（底白 0.10）+ 居中 14×14 指示灯：亮=`Allocate_SefiraGreen`、暗=灰 0.25、禁用=灰 0.4；`interactable=false` 灰显，文字/头像 ×0.4 |
| 字体 | `GlobalGameManager.currentLanguageFont?.GetFont(FontType.CONTEXT)?.font`，null 兜底 `Font.CreateDynamicFontFromOSFont("Microsoft YaHei", 16)`（静态缓存）。cn 玩家的 currentLanguageFont 实为 en 动态 OS 字体条目，能渲染汉字（`GetLanguageFont` 把 "cn" 重映射到 "en"） |
| 字号（参考 480px 宽面板） | 标题 22 / 行名 18 / 组标题与按钮 16 / 次级 15 / 提示 15 |
| 尺寸 | 面板宽 480、标题栏 44、行高 48、组标题 30、头像 36、留白 12 |

`UIColorManager` 可取色清单（`instance` 惰性 FindObjectOfType）：`RiskLevelColor[]`、`RWBPTypeColor[]`、`uiTextDefColor`、`UINormalColor/UIBlueColor/UIRedColor/UIDisabledColor`、`orange`、`EscapedColor`、`OverloadColor[]`。

## 素材策略（定稿，三路线结论）

1. **偷游戏 sprite：对外框不可行**——游戏 UI 类的 public Sprite 字段全是内容图标（CommandWindow.Work_*、DeployUI.AgentGradeImage 等），无窗口底图/边框。字体有先例可偷（ModList 偷 `OptionUI.Instance.CreditTitle.font`），但底图没有。
2. **程序化生成**（Texture2D SetPixels + Sprite.Create）：唯一认可的兜底，纯代码、单 DLL 分发不变。当前未用——扁平纯色已够。
3. **随包发行 PNG：拒绝**（BaseMod 从磁盘加载的路线，分发复杂度不值）。

## 纯代码 UI 惯用法（反编译树范本）

| 模式 | 范本（lobotomy-decompiled/Assembly-CSharp/） |
|---|---|
| 行 = Image+Button+闭包捕获索引 | `ModList.cs:324-341` |
| 行内 Text + anchor 定位 | `ModList.cs:289-311` |
| 固定行自上而下排（localPosition 递减） | `ModList.Awake` 181-210（5 行分页） |
| EventTrigger PointerDown/Drag | `SefiraPanel.cs:1227-1260` |
| 清空列表（遍历父 transform Destroy） | `Inventory/InventoryAgentController.cs:205-226` |
| 限高滚动列表 | ScrollRect 挂 viewport 对象（`horizontal=false, Clamped, scrollSensitivity=24`）+ `RectMask2D` 裁剪 + viewport 透明 Image **raycastTarget=true** 承接滚轮；content 锚 (0,1)-(1,1) pivot(0.5,1)，Rebuild 末回写 sizeDelta 并把面板高 cap 到 `TitleBar+MaxContentHeight+Hint`。长列表实用定式 |
| 覆盖式弹窗 + 黑遮罩 | `LobotomyBaseMod/GlobalNoticeBox.cs:25-52` |
| 手动射线穿透判定 | `NothingSnipingUI.cs:36-42` |

要点：
- **挂点**：游戏内覆盖 UI 挂 `GameObject.FindGameObjectWithTag("GlobalCanvas").transform`（TextAppearEffect 同款）；`GlobalGameManager.instance == null`（主菜单）直接不建。备选 `GlobalGameManager.instance.gameObject.transform.GetChild(0)`。
- **无需建 Canvas**：游戏从不从代码建 Canvas，全树无 `AddComponent<Canvas>`。跨 frame 销毁重建安全（回主菜单 GlobalCanvas 随场景销毁，静态 `_instance` 在 OnDestroy 置 null）。
- **暂停/倍速安全**：UGUI EventSystem/onClick 不依赖 `Time.timeScale`（SettingWindow 暂停时可点为证）；凡计时/刷新用 `Time.unscaledDeltaTime`。
- **拖动坐标换算**：`anchoredPosition += (mousePosition - startMouse) / canvas.scaleFactor`（overlay canvas 有缩放时鼠标是屏幕像素）。
- **raycastTarget 纪律**：只有吃点击的 Image（行底/标题栏/按钮）保留 raycast，其余（文字/头像）全部 `raycastTarget = false`，否则挡住父级行点击。
- **Text 防换行**：`horizontalOverflow = HorizontalWrapMode.Overflow`。

## UGUI 程序集链（大坑）

`Image/Text/Button/EventTrigger` 在 `UnityEngine.UI.dll`，而它是对 **monolithic facade** 编译的，mcs 必须补全传递闭包，缺一报 CS0006/CS0012/CS1070：

```
-r:lib/UnityEngine.dll                      # facade（type-forwarder）
-r:lib/UnityEngine.CoreModule.dll -r:lib/UnityEngine.AnimationModule.dll -r:lib/UnityEngine.ImageConversionModule.dll
-r:lib/UnityEngine.TextRenderingModule.dll  # Font/TextAnchor 在这，不在 CoreModule
-r:lib/UnityEngine.UIModule.dll             # ICanvasRaycastFilter 在这
-r:lib/UnityEngine.UI.dll                   # uGUI 本体
```

共 9 个 DLL（原 5 + facade/TextRendering/UIModule/UI）。三处同步：本地 `lib/`、`.git/hooks/pre-commit`、`.github/workflows/ci.yml`（+ build-wsl.sh）。DLL 从游戏 `Managed/` 拷贝（模块 DLL 非铁律物，铁律只锁 Assembly-CSharp.dll）。新 UI 文件必须登记进 csproj `<Compile Include>`（mcs 清单从 csproj grep）。

## 文案

UI 界面文本 ≠ Angela 台词：平实即可，不受语气规范约束（见 angela.md 红线）。键名走 `{区域}_{键}` 惯例入 LanguageBase `#region`，繁中默认继承简中。命名弃直译——「farm」在文案里定名**「轮班」**（Work Roster），按设施管理场景造词。
