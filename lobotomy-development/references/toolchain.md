# Toolchain（编译/测试/部署/git）

## 编译原理

- 游戏是 Unity 2017.4.7f1，脚本运行时 **.NET Framework 3.5**；"Unity Full v3.5" targeting pack 已绝版
- 替代：微软官方引用程序集 nupkg `microsoft.netframework.referenceassemblies.net35`（nuget.org 下载解压）→ `~/.local/share/net35-refasm/build/.NETFramework/v3.5`
- **xbuild 不可用**（不认 FrameworkPathOverride，报 CS0518），用 **mcs 手动编译**：

```bash
REF="$HOME/.local/share/net35-refasm/build/.NETFramework/v3.5"
mcs -noconfig -nostdlib -target:library -optimize+ -langversion:latest \
    -r:"$REF/mscorlib.dll" -r:"$REF/System.dll" -r:"$REF/System.Core.dll" -r:"$REF/System.Xml.dll" \
    -r:lib/0Harmony.dll -r:lib/Assembly-CSharp.dll \
    -r:lib/UnityEngine.dll -r:lib/UnityEngine.CoreModule.dll -r:lib/UnityEngine.AnimationModule.dll -r:lib/UnityEngine.ImageConversionModule.dll \
    -r:lib/UnityEngine.TextRenderingModule.dll -r:lib/UnityEngine.UIModule.dll -r:lib/UnityEngine.UI.dll \
    -res:Graph.xml,AutoInority.Graph.xml -out:输出.dll <csproj 里列出的 .cs>
```

- lib/ 共 9 个 DLL：原 5 个（0Harmony/Assembly-CSharp/CoreModule/AnimationModule/ImageConversionModule）+ UGUI 链 4 个（facade UnityEngine/TextRenderingModule/UIModule/UI，均为游戏 Managed/ 原版模块 DLL；不用 UGUI 时后 4 个可省）。引用细节见 ui.md

- .cs 列表从 csproj 抽取：`grep -o 'Include="[^"]*\.cs"' AutoInority.csproj | sed 's/Include="//;s/"$//' | tr '\\' '/'`

## 一键流程

- `lobotomy-company-addons/build-wsl.sh`——编译 AutoInority/RelocateAgents/TodayOrdeal 并部署到游戏 `BaseMods/inority_*_MOD/`（0Harmony.dll 从各 mod lib/ 取，路径用 dirname 链拼，**DrvFs 上 `..` 不可靠**）
- **pre-commit 钩子**（AutoInority/.git/hooks/pre-commit）：commit 前做上述编译检查，失败拦截；缺工具链静默跳过；自定位 ROOT（worktree 兼容）
- 测试：`cd AutoInority.Test && dotnet test`（net8.0，与 mod 的 net35 解耦）

## 部署与还原

- 部署目标：`/mnt/d/SteamLibrary/steamapps/common/LobotomyCorp/LobotomyCorp_Data/BaseMods/inority_*_MOD/`
- 原版备份：`Assembly-CSharp.dll.vanilla.bak`；**Steam 验证完整性会还原原版 → BaseMod 需重装**（见 basemod.md）
- 反编译工具：`DOTNET_ROLL_FORWARD=LatestMajor ~/.dotnet/tools/ilspycmd -t 类型全名 <dll>`（net6 程序跑在 net8 上需此环境变量）

## git 与 PR

- AutoInority **git 根在 `lobotomy-company-addons/AutoInority/`** 子目录；RelocateAgents/TodayOrdeal 各自独立仓库；父目录不是 git 仓库
- **PR 默认流程（2026-10-18 起固化）**：`git fetch origin` 取最新 main → 从 `origin/main` 开 **worktree**（`git worktree add`，用户工作区保持 `bugfixes` 不动）→ 改码/编译/测试/commit/push → 发中文 PR → **清理 worktree**（`git worktree remove`）
- 远端 `terencefan/LobotomyAutomation`（2022 年由 LabotomyAutomation 改名）；本地 30+ 分支，用户工作区常停 `bugfixes` 分支
- DrvFs 上所有文件显示 executable → 仓库已设 `core.fileMode false`，勿被 mode 噪声迷惑
- **commit / PR 标题正文一律中文**；本机 gh 老，`gh pr edit` 有 GraphQL bug，改 PR 用 `gh api repos/terencefan/LobotomyAutomation/pulls/N -X PATCH -f title= -f body=`
- 待合并链（2026-10-18）：#67 → #71 → #72 → #69 → #70（#70 与 #67 同行需 rebase）

## 常见坑速查

| 坑 | 对策 |
|---|---|
| lib/ 的 Assembly-CSharp.dll 被原版覆盖 → CS1061 | 恢复 BaseMod 补丁版（唯一合法来源是游戏 Managed/ 当前文件） |
| xbuild 报 CS0518 | 用 mcs 手动编译（上方模板） |
| DrvFs `..` 路径解析错误 | dirname 链拼接 |
| 编译产物仅 ~107KB | Graph.xml 没嵌（须 `-res:Graph.xml,AutoInority.Graph.xml`） |
| 中文用户看到英文文案 | SystemLanguage.Chinese（=6）没处理 |
| ilspycmd 报要装 .NET 6 | 前缀 `DOTNET_ROLL_FORWARD=LatestMajor` |
