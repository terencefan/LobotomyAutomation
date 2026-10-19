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

## 铁律

- **仓库 lib/ 里的 Assembly-CSharp.dll 就是补丁版**（编译引用它才有 GetUnitBufList/Add_On）。用游戏原版覆盖 = CS1061。原版备份在 `LobotomyCorp_Data/Assembly-CSharp.dll.vanilla.bak`
- **Steam "验证文件完整性" 会把 Assembly-CSharp.dll 还原成原版**——还原后必须重装 BaseMod（含 lib/ 引用同步），否则 mod 全挂
- lib/ 与游戏 Managed/ 的补丁版需保持同一份；升级 LMM 时两处一起换

## 反编译参考树的关系

`~/workspace/lobotomy-decompiled/Assembly-CSharp/`（2561 .cs）即从**补丁版**反编译——树里能看到 BaseMod 注入的 API 与原版游戏代码混排；`~/workspace/lobotomy-decompiled/LobotomyBaseModLib/` 是 BaseMod 自身库。

## mod 侧依赖方式

AutoInority 直接引用补丁版 Assembly-CSharp.dll（不用 LobotomyBaseModLib 的加载器接口），以 Harmony 1.x 补丁 + `Add_On` 入口挂进游戏（见 autoinority.md）。
