# GameManager（全局管理器）

路径均相对 `~/workspace/lobotomy-decompiled/Assembly-CSharp/`。

## GameManager（GameManager.cs）——局内管理器，MonoBehaviour

- 访问：静态 `GameManager.currentGameManager`（另有 `GameManager.instance`，未验证并存关系）
- `int gameSpeedLevel = 1` + `SetPlaySpeed(int level)`——1/2/3 倍速
- `GameState state`（GameState.cs）：`PLAYING / STOP / PAUSE`
- `bool ManageStarted`（管理阶段开始）、`bool emergency`（紧急状态）
- `Pause() / Resume()`、`StartStage() / StartGame() / EndGame()`

## GlobalGameManager（GlobalGameManager.cs）——跨局管理器，单例

**语言系统（AutoInority 中文化的关键，已验证）**：

- `SystemLanguage Language { get; set; }`（私有 `_language`，默认 English）
- 来源：`public string language = "en"` 字段，`Awake()` 里按值 switch（"vn"/"bg"/"ru"/"es"/"jp"/"kr"/"cn_tr"/"cn"/默认 en）
- **坑："cn" 映射到 `SystemLanguage.Chinese`（=6，legacy 值），不是 `ChineseSimplified`（=40）**——mod 按 Simplified 判断会漏掉中文用户，必须同时处理 `Chinese`（AutoInority Angela.cs 已修）
- 语言切换事件：`NoticeName.LanaguageChange`（**拼写坑：Lanaguage**）
- 其他：`gameMode`（GameMode）、`dlcCreatureOn`、`currentLanguageFont`

## GameSettingModel（GameSettingModel.cs）——单例

分辨率/画质：`InitResolution()`、`GetCurrentTextureQuality()/SetCurrentTextureQuality(int)`、`GetResolutionList()`、`GetCurrentFullScreenMode()`、`SetResolution(int, int, GameFullScreenMode)`。

## 未涉及但存在的全局（索引）

- `SefiraBossManager`（Hod 战等 Sefira Boss 状态，AgentModel.cs L140 引用）
- `CalendarManager`（日/日程驱动，触发 Ordeal）
- 存档：GlobalGameManager 的 `save*FileName` 系列字段（文件名规律未验证）

## AutoInority 使用处

- `Angela.cs`：`GlobalGameManager.instance.Language` 切换三语文案（Chinese case 修复见上）
- TodayOrdeal：读日历/ GameManager 状态决定推送时机
