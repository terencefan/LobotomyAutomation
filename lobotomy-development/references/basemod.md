# BaseMod（mod 加载框架）

脑叶**没有官方 mod 加载器**，BaseMod 是社区事实标准（Nexus Mods #2 位，"Lobotomy Mod Manager" 系）。

## 安装原理（本机已装，LMM 1.3.9）

- 分发：GitHub `LobotomyBaseMod/LMM` 的 release（Nexus 页面无独立文件）
- **替换 `LobotomyCorp_Data/Managed/Assembly-CSharp.dll` 为补丁版**：注入 `Add_On` 类、`GetUnitBufList` 等 mod API、BaseMod 资源路径
- 附带 DLL：`LobotomyBaseMod.dll`、`NAudio.dll`、`Facepunch.Steamworks.Win64.dll`、`steam_api64.dll` + `Managed/BaseMod/` 数据目录
- 补丁版校验：md5 前缀 ad13b645（2026-10-18 装的 1.3.9）

## 目录约定

- mod 安装位置：`LobotomyCorp_Data/BaseMods/<mod名>/`，每目录一个 DLL + 资源
- 日志：`LobotomyCorp_Data/BaseMods/*.txt`（排查加载失败先看这里）

## 名册接管与内容来源分层（2026-10-20 原版供给表验证）

`BaseList.txt`（`Managed/BaseMod/`）**整册接管**异想体名册：128 条从 100000（Dummy）起重列全部可获异想体，含原版正册成员；反编译树与普查所引用的正册成员 stat/脚本都是 BaseMod 收录版。

**原版代码的供给结构**（`CreatureGenerateInfo`，反编译自原版备份程序集）：

- `all`（82）/ 图鉴 `all_for_codex`（74）包含全部争议 id（100101/100102/100103/100104/300106-300110）——**没有「被砍未上架」批次**。
- `r1`-`r6` 随机到货表含 100101 沉默的代价（r5/r6 全表）、100102 冰雪女皇（r2/r3/r5/r6）；魔弹射手/阴只在 all+图鉴，与大多数剧情解锁成员同待遇（CENSORED/碧蓝新星同样不在 r 表）。
- **唯一的原版内容开关**：`GlobalGameManager.dlcCreatureOn` + `creditCreatures`（7 只，DLC 关闭时从 `GetAll` 剔除）——粉红「小分队」100064、溶解之爱 100063、世界树 100062、月光女神 100065、风云法师 100105、波迪 100106、回溯之钟 300110（WonderLab 系）。
- 原版 `kitCreature` 13 件工具：300001-300007、300101-300105、300108；荧光手镯 300106、行为矫正仪 300107 不在其列。

**stat 完整性先行（沉默的代价教训）**：沉默的代价是**原版开发中途被砍的半成品**——原版本体 `resources.assets` 的 `xml/creatureStats/ScytheClock_stat` TextAsset 里 stat 块就只有 `<qliphoth>0</qliphoth>`（对照 `Lumberjack_stat` 有完整 workProb 四型/心情三线/workDamage/maxWorkCount），类代码同样写一半（`KillOnce(float)` 参数、`FEELING_UP_AMOUNT` 未接线死代码），到货表 r5/r6、图鉴与本地化 xml 却收录了它；BaseMod 的 stat txt 是这份残根的**忠实搬运**，不是 BaseMod 偷懒。后果：实机工作概率全 0、心情恒 NONE、「暂停处刑」机制从未武装。**凡「能不能派工/机制是否可达」的结论，先查 stat 完整性再下断言；「进了到货表/图鉴」「类在原版程序集里」都不是启用证据**（全集 82 只的类全在）。

原版 stat TextAsset 抠取：在 `resources.assets` 里 `grep -abo "<名>_stat"` 定位 → 按布局解析（名字串 + **按 4 字节对齐的填充** + 4 字节内容长度 + 正文；手写 dd 容易踩对齐坑，用 python mmap 脚本批量核对更可靠）。

**全量 stat 审计结论（2026-10-20，BaseList 116 条逐条核对工作管线字段）**：

- **原版中途被砍的半成品只有两只**（沉默的代价、柳橙树）——被砍形态明细专表见 creatures-zh.md「原版被砍异想体」。
- **看似缺字段实为设计**：阳（`workType=kit`+`kitType=equip`，是阴的配套工具，非残根）、回溯之钟（`workType=kit`+`oneshot`，credit DLC）、光之锤/别碰我/穷编剧手册（礼物/处刑脚本驱动，不吃标准 workProb）；冰雪女皇/魔弹射手/阴/波迪/风云法师/月光女神/溶解之爱/世界树/粉红小分队 9 项字段全 = 完整成品。
- **命名笔误**：秃子的 stat 资产名是小写 `bald_stat`，CreatureList 写 `Bald_stat`——Windows 大小写不敏感才没炸；跨平台工具按名检索时要注意。

BaseMod 升级 = BaseList/stat 全量漂移：反编译树、stat 普查结论、lib DLL 同时过时，「测试绿但实机已变」窗口以各 PR 实机验收清单对冲。

## 铁律

- **仓库 lib/ 里的 Assembly-CSharp.dll 就是补丁版**（编译引用它才有 GetUnitBufList/Add_On）。用游戏原版覆盖 = CS1061。原版备份在 `LobotomyCorp_Data/Assembly-CSharp.dll.vanilla.bak`
- **Steam "验证文件完整性" 会把 Assembly-CSharp.dll 还原成原版**——还原后必须重装 BaseMod（含 lib/ 引用同步），否则 mod 全挂
- lib/ 与游戏 Managed/ 的补丁版需保持同一份；升级 LMM 时两处一起换

## 反编译参考树的关系

`~/workspace/lobotomy-decompiled/Assembly-CSharp/`（2561 .cs）即从**补丁版**反编译——树里能看到 BaseMod 注入的 API 与原版游戏代码混排；`~/workspace/lobotomy-decompiled/LobotomyBaseModLib/` 是 BaseMod 自身库。

## mod 侧依赖方式

AutoInority 直接引用补丁版 Assembly-CSharp.dll（不用 LobotomyBaseModLib 的加载器接口），以 Harmony 1.x 补丁 + `Add_On` 入口挂进游戏（见 autoinority.md）。
