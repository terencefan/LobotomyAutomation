# AutoInority（mod 架构）

仓库：`/mnt/c/Users/Terence/workspace/lobotomy-company-addons/AutoInority/`（**git 根在此目录**，远端 `terencefan/LobotomyAutomation`）。工程目录 `AutoInority/AutoInority/`（源码）+ `lib/`（引用 DLL）+ `Graph.xml`（必须以 `AutoInority.Graph.xml` 嵌入，漏嵌产物仅 ~107KB，正常 ~9.4MB）。

## 启动与主循环

- **Harmony_Patch**（Harmony 1.x）：补丁清单挂游戏事件（OnAgentPanic、work 流程等）→ 初始化 Automaton
- **Automaton**（Automaton.cs，单例 `Automaton.Instance`）：主循环 `Main()` 每帧跑；决策核心
  - `Main()` 流程（2026-10-18 重构为**无状态调度**，PR #78）：每 tick 从源头重建队列——`FarmingCreatures` 种地 + 过载 kit（ManageKitCommand）→ 紧急收容物（FilterUrgent → ManageCreatureCommand）→ 出逃/考验（SuppressCommand）→ PriorityQueue 排空即丢弃。旧 `_repeatQueue/_commands` 模型的去重集合只删不增，失败命令每 tick 累积一份（O(T²)），已删除
  - `InEmergency`：考验在场 && 有过载收容物
  - `Register/Remove(agent, creature, skill)`：员工↔工作绑定，挂 AutomatonBuf，`Notice.Send("AddSystemLog", ...)` 通知（不再直接入队，下 tick 生效）
  - 置信度阈值：`CounterDecreaseConfidence 0.95 / CreatureEscapeConfidence 0.99 / DeadConfidence 0.99`（hardcoded，无持久化）
- **Command 体系**（Command/）：优先级队列（`BaseCommand.CompareTo` 反转 + 游戏最小堆 = 高优先级先出）；`RepeatWorkCommand`（连续工作）、`ManageCreatureCommand`、`ManageKitCommand`、`FarmCommand`、`SuppressCommand`；`IsApplicable/Execute/IsCompleted`；PriorityEnum 中 `MANAGE_CREATURE_4/5_URGENT` 压过 `RESCUE_PANIC_AGENT`（1~3 级紧急压不过）——刻意保持的设计
- **选人链路**（2026-10-18 深审）：`Candidate.Suggest`（异想体×职工×SkillSets 三重循环，过滤 = `CanWorkWith` + `CheckConfidence`）→ Sort → First。`ManageComparer` 曾有 y 侧置信度误用 x 的值的复制错误（择优完全失效，PR #77 修复）；`IsCapableOfPressing` 曾把护甲读成武器 + 防御行恒取 "1"（PR #79 修复）；`IsAvailable` 满血满精神要求放宽为 90% 阈值（PR #81，与 CheckSurvive 双重保守解耦）；死代码 FindCandidates/FindAgents(80)/FilterCanSuppress 已删（PR #80）
- **白夜联动安全**（PR #82，2026-10-18）：`OneBadManyGoodExt.CanWorkWith` 在白夜（100015）逃逸时**无条件**拒绝对一罪与百善派工（优先于过载放行——一罪无计数器，过载爆炸只扣能源，换不得「失乐园」）——游戏 `UseSkill.OnSkillGoalComplete` 满箱不查工作类型即触发赎罪处决（月记 bug，被处决白夜不掉「失乐园」），自动化种地几乎必中；`IsAvailable` 同时排除带 `DEATH_ANGEL_BETRAYER` buf 的叛徒（无敌、仅限赎罪、绕过指令窗口检查）。「顺水推舟自动处决」方案以 TODO 留在扩展类。要点：**`CanWorkWith` 是初派+循环两路径的公共咽喉，`AgentModel.ManageCreature` 程序化派工不经过指令窗口的 OnWork 检查**
- **种地链路审查**（PR #83，2026-10-18）：核心逻辑正常（流水线补员=游戏 `IsolateRoom.OnEnterRoom` 原生语义、关窗后 CurrentWindow 常驻不 NRE）。X 键双模式：窗口开→AssignWork 单次择优派工，无窗→TryTrain 单次练级——**排序保持 FarmComparer**（用户定案：单次派工同样吃饰品/经验收益，曾改 ManageComparer 后回退）；T 开关播报**保持 Log**（用户定案：Log=通告平实体、Say 气泡必须有性格、Announce 仅限带性格文案，详见 angela.md 红线；曾升 Announce 后回退）。T 链路 10 问定案保持现状：在途不取消、出逃期照常、60 帧节流、需开窗、手动取消=关种地
- **快捷键绑定表**（`Harmony_Patch._keyBindings`，2026-10-19 定型）：`KeyBinding{Key, OnDown}` 两字段 struct + List，主循环只做 Down 单相位分发，**加键=加一行**。全表：`P` 总开关 / `T` 种地开关（开窗选中异想体时；曾短暂改 F + 单击/长按区分，#85 回退 T 并删除该机制）/ `X` 单次派工或练级 / `O` 轮班管理面板 / `V` 清空 / `Z` 调试日志
- **异想体管理面板 v2**（UI/FarmPanel.cs，#86→main bff9dbf，2026-10-19）：**O 键**开关的 UGUI 纯代码面板。v2 定位升级：**设施全量异想体列表**（`CreatureManager.GetCreatureList()` 滤 `IsCreature()`+非 OrdealCreatureModel），每行独立**开发开关**（=farm toggle，走 `ToggleFarming`；出逃/镇压行 `interactable=false` 灰显——判定 `IsEscaped()||state==SUPPRESSED(_RETURN)`，**勿用 mod 的 IsAvailable**（其语义=空闲可派工））；部门⇄等级双视角，组 section 组色线框（程序化九宫格 Sliced）；行区 640px 限高 ScrollRect 滚动；行=头像+风险色名+职工+开关灯；拖动、unscaled 1s 刷新。文案 v2 定名：「异想体管理」/Creature Management，farm 措辞统一**「开发」**（v1 曾叫「轮班」）。挂点定案：GlobalCanvas tag 对象存在但**不渲染**（唯一引用 TextAppearEffect 是死代码），用 `GlobalGameManager.transform.GetChild(0)`（BaseMod GlobalNoticeBox 同款）。静态 `Toggle()` 唯一入口；OnDestroy 清 `_instance`。**做游戏内 UI 先读 ui.md**（设计语言/素材策略/UGUI 程序集链坑）

## 异想体扩展层

- `ICreatureExtension` + `Creature/` 目录 **75 个 `XxxExt : BaseCreatureExt`**（每异想体一个），`BuildExtension` 按 game script 类型分发映射
- `BaseCreatureExt`（Creature/BaseCreatureExt.cs）：公共警告逻辑（`Angela.Creature.*` 文案）、`CheckSurvive()` 生存建模（装备修正白名单 + 护盾池 + B/P 特殊语义）、`MentalFix` 虚方法（默认恒等，CensoredExt 0.4×）

## 数学层与消息层

- **Confidence**（Confidence.cs）：工作生存概率估计，语义见 confidence.md 与 work.md
- **Angela**（Angela.cs）：`Say`（气泡）/`Log`（系统日志）/**`Announce`（二合一）**；三语切换（含 SystemLanguage.Chinese 坑）；`Angela.Creature.*` 硬编码中文警告
- **Localization/**（LanguageBase 三语资源类）

## 兄弟 mod（同仓库根，各自独立 git 仓库）

- `LobotomyRelocateAgents/`——员工搬家
- `LobotomyTodayOrdeal/`——当日考验播报（系统日志风格）
- 一键编译部署三者：仓库根（lobotomy-company-addons）`./build-wsl.sh`
