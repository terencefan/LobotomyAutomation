---
name: lobotomy-development
description: 脑叶公司（Lobotomy Corp）mod 开发参考手册：游戏原子实体（异想体/工作/伤害/buf/地图/事件）、BaseMod 框架、net35 编译工具链。当提到脑叶、Lobotomy、LobotomyCorp、AutoInority、inority、异想体、收容异常、逆卡巴拉、给脑叶写 mod/插件时使用
---

# 脑叶公司 mod 开发

三个代码位置，先分清再动手：

| 位置 | 路径 | 用途 |
|---|---|---|
| mod 仓库 | `/mnt/c/Users/Terence/workspace/lobotomy-company-addons` | AutoInority（git 根在其子目录 `AutoInority/`）、RelocateAgents、TodayOrdeal |
| 反编译参考树 | `~/workspace/lobotomy-decompiled/Assembly-CSharp/`（平铺 2561 个 .cs）+ `../LobotomyBaseModLib/` | 查任何游戏 API 先 grep 这里 |
| 游戏本体 | `/mnt/d/SteamLibrary/steamapps/common/LobotomyCorp` | 部署目标；BaseMod 已装，原版备份 `LobotomyCorp_Data/Assembly-CSharp.dll.vanilla.bak` |

## references 索引（一实体一文件，按需读取）

| 资料/横切 | 文件 | 内容 |
|---|---|---|
| 中文 wiki | `wiki.md` | fandom 资料源（api.php 用法）、译名/数据权威规则、术语对照表（工作类型↔RwbpType 代码已验证）、编号规则 |
| 异想体译名 | `creatures-zh.md` | 全量 101 条：script 类名↔游戏 id↔简体↔繁体↔wiki 页面名（含编号） |

| 游戏实体 | 文件 | 内容 |
|---|---|---|
| 单位 | `unit.md` | UnitModel→WorkerModel→AgentModel 链、四维属性↔RwbpType、GetWorkSuccessProb |
| 装备 | `equipment.md` | UnitEquipSpace（无 EquipmentInventory 类）、OnTakeDamage(ref)、gifts |
| 异想体 | `creature.md` | CreatureModel 数据面：qliphothCounter/feelingState/state/GetRiskLevel |
| 异想体脚本 | `creature-base.md` | CreatureBase 生命周期钩子签名表（带行号） |
| 工具箱 | `kit.md` | KitEquipEventListener(kitEvent)、CreatureKitType |
| 工作 | `work.md` | UseSkill 结算：n=maxCubeCount 次 Bernoulli + U[min,max] 伤害（置信度建模依据） |
| 伤害 | `damage.md` | DamageInfo、六环管线顺序、B 双扣/P %maxHp |
| Buf | `buf.md` | UnitBuf、BarrierBuf 反射读法（UseBarrier 有副作用） |
| 事件 | `notice.md` | Notice.Send/Observe（无 Publish）、NoticeName 常用表 |
| 部门 | `sefira.md` | SefiraEnum 13 值（TIPERERTH 拼写坑）、agentDeadPenalty |
| 地图 | `map.md` | MapGraph/MapNode/PassageObjectModel（GetEnteredTargets） |
| 考验 | `ordeal.md` | OrdealCreatureModel（构造即 ESCAPE、Escape 被禁用） |
| 全局管理器 | `gamemanager.md` | GameManager 倍速、GlobalGameManager 语言映射（"cn"→Chinese=6 坑） |
| 游戏内 UI | `ui.md` | UGUI 纯代码构建：设计语言定稿（调色板+字体+扁平深色）、素材三路线结论、ModList 惯用法表、**UGUI 程序集链 9 DLL 坑** |
| Angela | `angela.md` | AddAngelaMessage、AngelaMessageState、mod 文案体系、**语气规范（新文案必读）** |
| BaseMod 框架 | `basemod.md` | 补丁版 Assembly-CSharp 原理、BaseMods 目录、还原/重装 |
| AutoInority 架构 | `autoinority.md` | Harmony 补丁→Automaton 主循环→Command→75 扩展类 |
| 置信度模型 | `confidence.md` | Confidence 数学结构、×2 bug 修复史、Irwin-Hall 候选 |
| 工具链 | `toolchain.md` | mcs 模板、build-wsl.sh、pre-commit、git/PR 规范、坑速查表 |

## 常用工作流

**① 查游戏 API**：`grep -rn "关键词" ~/workspace/lobotomy-decompiled/Assembly-CSharp/` → 读对应 .cs。单个类型快速反编译：`DOTNET_ROLL_FORWARD=LatestMajor ~/.dotnet/tools/ilspycmd -t 全名 <dll路径>`。查中文译名/异想体背景/场景描述 → `wiki.md`（api.php 用法）+ `creatures-zh.md`（类名↔译名↔wiki 页面名）。

**② 改码 → 编译 → 部署**：改 `AutoInority/AutoInority/*.cs` → 仓库根 `./build-wsl.sh`（编译三 mod 并部署进游戏）→ 游戏内验证。AutoInority 的 pre-commit 钩子会做 net35 编译检查（缺工具链时静默跳过）。

**③ 提交/PR**：commit 与 PR 标题正文一律中文；gh 版本旧，改 PR 用 `gh api repos/terencefan/LobotomyAutomation/pulls/N -X PATCH`。游戏公式相关改动跑 `cd AutoInority.Test && dotnet test`。

## 铁律

1. **lib/ 里的 Assembly-CSharp.dll 是 BaseMod 补丁版**（含 Add_On/GetUnitBufList），绝不能用游戏原版覆盖——覆盖后编译报 CS1061，唯一恢复来源是游戏目录的补丁版。
2. **xbuild 不可用**（不认 FrameworkPathOverride，CS0518），必须 mcs 手动编译，模板见 toolchain.md。
3. **DrvFs（/mnt/c、/mnt/d）上 `..` 路径解析不可靠**，脚本里用 dirname 拼接。
4. 改任何游戏公式（工作概率、伤害、计数器）前，先读对应 reference 条目的"已验证"结论，不要凭感觉建模。
5. **中文译名/文案表述以 wiki 为准，数值/机制以代码为准**（裁决规则见 wiki.md）；新异想体支持先在 creatures-zh.md 入表。
5. 反编译树是平铺结构，没有命名空间目录；类名即文件名。
