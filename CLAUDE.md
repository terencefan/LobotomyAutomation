# AutoInority 开发快速开始

Lobotomy Corporation 的自动化 mod（BepInEx + Harmony 1.x）：自动派工、异想体管理面板、门口弹回门检。
产品工程是老式 net35 csproj（Unity 2017.4 / mcs 编译），测试工程 net8 xunit。

## 环境事实

- 本仓库 git 根即本目录
- **没有游戏也能开发**：lib/ 九件套 DLL 已入库（含游戏现版 Assembly-CSharp），编译、测试、反编译全离线自足；游戏本体只在实机验收 / 部署 mod 时才需要
- 游戏安装路径不固定（需要时先探测，记为 `$GAME`）。Steam AppID 568220，目录名 `LobotomyCorp`；从 Steam 各库的 `libraryfolders.vdf` 找：

```bash
for vdf in "$HOME/.steam/steam/steamapps/libraryfolders.vdf" \
           "/mnt/c/Program Files (x86)/Steam/steamapps/libraryfolders.vdf"; do
  [ -f "$vdf" ] && grep -oP '"path"\s+"\K[^"]+' "$vdf"
done | sort -u | while read -r p; do
  p=$(printf '%s' "$p" | sed 's|\\\\|/|g; s|^C:|/mnt/c|; s|^D:|/mnt/d|; s|^E:|/mnt/e|')
  [ -d "$p/steamapps/common/LobotomyCorp" ] && echo "$p/steamapps/common/LobotomyCorp"
done
```

  兜底：各盘 `*/Steam*/steamapps/common` 下 `find -maxdepth 3 -iname LobotomyCorp`。已装 BaseMod/LMM，mod 部署在 `$GAME/LobotomyCorp_Data/BaseMods/`；Steam「验证完整性」会还原游戏本体 DLL，不影响仓库
- 游戏原子实体、BaseMod 原理、工具链细节、坑速查全在 skill `lobotomy-development`（本目录 `lobotomy-development/` 软链自 `~/.claude/skills/`，已入库）；**涉及游戏机制先读它**，references/ 有 21 篇分主题笔记（creature/buf/angela/ui/toolchain/wiki 等）

## 编译

- 一键编译部署：`./build-wsl.sh`（主工作树，编当前分支）
- 手动 mcs（CI build-net35 job 同款）：

```bash
cd AutoInority && REF="$HOME/.local/share/net35-refasm/build/.NETFramework/v3.5"
mcs -noconfig -nostdlib -target:library -optimize+ -langversion:latest \
  -r:"$REF/mscorlib.dll" -r:"$REF/System.dll" -r:"$REF/System.Core.dll" -r:"$REF/System.Xml.dll" \
  -r:lib/0Harmony.dll -r:lib/Assembly-CSharp.dll -r:lib/UnityEngine.dll -r:lib/UnityEngine.CoreModule.dll \
  -r:lib/UnityEngine.AnimationModule.dll -r:lib/UnityEngine.ImageConversionModule.dll \
  -r:lib/UnityEngine.TextRenderingModule.dll -r:lib/UnityEngine.UIModule.dll -r:lib/UnityEngine.UI.dll \
  -out:/tmp/out.dll \
  $(grep -o 'Include="[^"]*\.cs"' AutoInority.csproj | sed 's/Include="//;s/"$//' | tr '\\' '/')
```

- pre-commit 钩子自动跑 mcs 编译检查，编译挂会 block commit

## 测试（离线实测，无游戏依赖）

```bash
cd AutoInority.Test && dotnet test        # 89 用例全绿预期
../ci/local-gate.sh                       # 覆盖率门禁：diff 行 ≥80%（对齐 CI）
```

- 架构：测试工程**挂载真游戏 DLL**，通配链入产品源码——编译期符号对齐实机 API，无手写 stub
- 桥接层 `AutoInority.Test/Bridges/GameBridge.cs` 三种供给：①跳构造器分配 + 反射注字段（字段名 = 反编译锚点）②惰性单例 public API 直注 ③产品虚缝子类覆写
- 两堵原理墙（别再撞）：Unity Object 托管分配后 `== null` 恒真（fake null 走 native 壳）；icall（如 `Animator.GetInteger`）无 native 实现可调——引擎耦合叶子只能开产品虚缝（先例 `ShyLook.FaceTypeAtDoor`）
- 涉全局单例（SkillTypeList/Notice/Automaton.Instance）的测试类须入同一个 xunit `[Collection]`，对单例只增不删

## 铁律（违反会被用户纠正）

- **一切改动走 PR，由用户亲手合并**——绝不自合、绝不直接 push main，未经当次明确指示不 push
- PR / commit 文案一律中文；**一 PR 一提交**（rework 用 `git reset --soft` + `push -f`），直对 main 出 diff，**不要叠未合分支上链**
- 改已有 PR 走 `gh api REST`（`gh pr edit` 有 bug）
- 产品代码只为可测性提纯，绝不为测试改行为；stub/测试基建只进测试工程
- 文件编码注意：部分旧文件是 BOM+CRLF（如 Angela.cs）——字节级编辑（python），新文件 LF 无 BOM

## 代码导览

- `AutoInority/Creature/BaseCreatureExt.cs` — 异想体扩展基类（置信度链/门检 `ViableAtDoor`/buf 排除表 `BufRules`）
- `AutoInority/Creature/{ZAYIN,TETH,WAW,HE,ALEPH}/` — 各风险级 ext；`ShyLookPolicy.cs`/`BufExclusionRule.cs` 是可测提纯先例
- `AutoInority/Angela.cs` + `Angela.Say.cs` + `Angela.Log.cs` — 播报文案三件套（Say=主动播报，Log=客观输出，Log 带 verbose 形参仅 Z 键 debug 开启后发出）
- `AutoInority/Harmony_Patch.cs` — Harmony 挂点（门检在 `UseSkill.InitUseSkillAction` prefix）
- `AutoInority/Automaton.cs` — 调度核心（纯托管 partial，测试可直接实例化）

## 深水区

- 反编译参考树：`~/workspace/lobotomy-decompiled/Assembly-CSharp`（2561 个 .cs 平铺）；或现 dump：`DOTNET_ROLL_FORWARD=LatestMajor ilspycmd lib/Assembly-CSharp.dll > all.cs` 后 grep（工具要 .NET 6、本机是 8，roll-forward 绕开；嵌套类型 `-t` 不可靠）
- 中文基准 wiki 译名表：skill 的 `references/creatures-zh.md` / `wiki.md`
- 游戏本地化 XML：`$GAME/LobotomyCorp_Data/ExternalData/xml/Language/{cn,cn_tr}/`（需游戏本体）
- 测试反射字段名是反编译锚点——游戏更新后测试挂了先怀疑游戏改了私有字段（报 `MissingFieldException` 即此）

## 进行中

- 待 review / 合并的改动以 `gh pr list --state open` 为准——本文件不枚举 PR 号（枚举必过时）
- 已知长期挂起：调度重构（每 tick 从源头重建队列，修 O(T²) 累积）用户暂缓
- 2026-10-19 已合入 main：挂载真 DLL 测试基建（桥接层 + 89 用例 + 覆盖率门禁）、游戏开发 skill 入库
