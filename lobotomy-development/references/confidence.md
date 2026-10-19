# Confidence（置信度模型）

`AutoInority/AutoInority/Confidence.cs`——AutoInority 判断"该不该派员工工作"的数学层。测试：`AutoInority.Test/`（net8.0 xunit，`dotnet test`，65 用例含 Monte Carlo 校准，种子固定 `Random.Shared`）。

## 模型结构（与游戏精确同构）

游戏语义（见 work.md）：一次工作 = `n = maxCubeCount` 次独立 Bernoulli(p)，p 由属性等级查表；失败伤害 i.i.d. U[min,max]（`isAttackInWorkProcess()` 为前提）。

模型回答的问题：**做完这次工作，员工死亡/出逃/计数器减少的概率是否低于阈值**（DeadConfidence 0.99 / CreatureEscapeConfidence 0.99 / CounterDecreaseConfidence 0.95）。

- 成功次数 k ~ Binomial(n, p)；损失 = (n−k) 次 U[min,max] 之和
- 现实现用 **CLT 正态近似**：`P()` 内 `Math.Pow` 计算 Φ；`NCR()` 组合数（越界抛 ArgumentOutOfRangeException）

## 2026-10-18 修复史（PR 已发，按序合并 #67→#71→#72→#69→#70）

- **#72 ×2 z-score bug**：原 `Confidence.cs:169` `(maxPoints - i*avgDamage)/delta*2` 把 z 分数乘了 2，Φ(2z) > Φ(z) → **系统性高估生存概率**（往危险方向偏）。正确 z = `((M−i·min)/δ − i/2)/√(i/12)`。修复后阈值实际变严——部分原本"接受"的工作会被拒，属预期。review 补丁（同 PR，7902ca9）：① NCR 守卫 `>`→`>=`（表 36 行只覆盖 n≤35，原放行 n=36 越界）；② δ=0 退化分支——min==max 且 i·damage 恰等 maxPoints−1 时原落 0/0=NaN（偏差平台相关：x64 −0.012/ARM64 −0.006），改为确定性存活全额计入
- **#69 装备修正层**：CheckSurvive 加 `ApplyEquipmentDamageModifiers`（script 类型白名单：DeathAngelArmor 非持 400015、NamelessFetusGift；有状态装备跳过）
- **#70 护盾建模**：`AgentModel.GetBarrierValue(RwbpType)` 反射读 BarrierBuf（A 型通用盾并入）；池：R→hp+barrier，W→MentalFix(mental)+barrier(W)，B→min(hp, MentalFix(mental))+barrier(B)，P→hp+barrier(P)。与 #67 同行需 rebase
- **#75/#76 测试复活（原 #71 拆分）**：#75 = net8.0 + source-link Confidence.cs/Log.cs + UnityEngine.CoreModule.dll 引用 + Random.Shared；#76 = 断言修复（stacked 于 #75）
  - 考古注记：2022 年旧测试 `Assert.Equal(0.1)`（Alriune/Silent 的 InRange）是**业务风险阈值误植为数学期望**（6~8 个不同 p 断言同一值，真值实为 0.55~0.91），不是实现错
  - 5 参 Survive 在 min=max 退化时 delta=0：混合区分支可能出现 0/0（NaN→分布表索引裁剪→≈0.002 贡献），已知无害怪癖，误差远小于容差
- **#67 B 伤害置信度**：B 双扣池建模（见 damage.md）

## 下轮候选（明确未做）

- **精确卷积**：CLT → Irwin-Hall（均匀分布和的精确分布），消灭正态近似误差
- **阈值配置持久化**：三个 Confidence 阈值目前 hardcoded，无设置界面/存档

## 测试运行时偏差防线（net8 测试 vs 游戏 net35）

测试项目是 net8、游戏是 mono net35，两者不对齐是**故意的**（net35 无现代测试运行器）。偏差被三道护栏压住，改动时勿破坏：

1. **双重编译**：pre-commit 的 mcs 验 mod 过 net35 API 集，`dotnet test` 验同一份 Confidence.cs 过 net8 API 集——API 符号漂移提交前必爆
2. **数学确定性**：`Math.Pow/Exp/Sqrt` 是 IEEE 确定性运算，跨运行时只有 ULP 级（~1e-16）差异，断言容差 1e-6，不可能翻转阈值决策
3. **统计断言**：三个运行时的 Random 算法互不相同（含游戏的 `UnityEngine.Random`），MC 用例只断言统计性质，**永远别写"固定种子断言精确值"的测试**——那是假对齐

**测试项目红线**：只 source-link 纯逻辑源文件（Confidence.cs/Log.cs），**永远不引用 mod DLL**（会把 net35 依赖全带进 net8）；游戏集成（Harmony/伤害管线）不归测试管，靠游戏内冒烟。精度换挡（`Mathf.Pow`→`Math.Pow` 这类 float→double）测试察觉不到游戏侧影响，改动时单独评估。

## CI（PR #84，2026-10-19）

`.github/workflows/ci.yml` 两个作业：

- **test**：stub 掉 UnityEngine.CoreModule（测试面仅 Log.cs 的 `Application.dataPath`，存根在 `ci/stubs/`）→ `dotnet-coverage collect` 采 cobertura（**XPlat Code Coverage collector 对 source-link 文件产出空报告**，勿再用）→ `diff-cover --fail-under 80` patch 门禁——只统计 PR 相对 origin/main 改动行且只看报告内文件，游戏耦合层天然免疫
- **build-net35**：mcs 构建检查（等价 pre-commit），lib 物料走私有仓库 release（`LIBS_TOKEN`+`LIBS_REPO` 未配置则优雅跳过；public 仓库禁止放游戏 DLL）
- 坑：验证门禁时**追加注释行不会出现在覆盖率报告**（coverlet 不测量注释）——看似"门禁失灵"实为正确行为；测失败路径要注入可执行且不可达的语句（如永不被调用的方法）
- 坑：`diff-cover` 的阈值参数是 `--fail-under` 不是 `--threshold`
