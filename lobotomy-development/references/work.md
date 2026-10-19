# Work（工作结算）

路径均相对 `~/workspace/lobotomy-decompiled/Assembly-CSharp/`。本文件是 AutoInority 置信度模型的直接依据，全部已对照源码验证。

## UseSkill（UseSkill.cs）——一次工作的完整生命周期

- L226-228：**每帧 tick 时 `workCount++`**；`workCount` 是**总 tick 计数**（不是剩余数）
- **工作总长恰好 `maxCubeCount` 个 tick**（生产完即结束）
- L536-601 `ProcessWorkTick`：每 tick 以 `GetWorkSuccessProb` 为概率做 **Bernoulli 试验**（`Random.value < workSuccessProb`）；失败时若 `script.isAttackInWorkProcess()`（默认 true）则按 `DamageInfo(min,max)` **均匀分布**取值造成伤害
- 成功 tick 产 cube；结束触发 OnFinishWork / WorkEndReport 事件
- `agent.Equipment.kitCreature.script.kitEvent.OnEnterRoom(agent, this)`（L164，kit 进入房间钩子）

## 结论（置信度建模语义）

一次工作 = **n = maxCubeCount 次独立 Bernoulli(p) 试验**，p = `GetWorkSuccessProb`（属性等级查表，见 unit.md）；
失败伤害 = i.i.d. **U[min,max]**（tick 间独立）；总失败伤害 = **Irwin-Hall 卷积**。
AutoInority Confidence.cs 现用 CLT 正态近似——结构上与游戏精确同构，差异只在数值近似（升级候选：精确卷积，见 confidence.md）。

## SkillTypeInfo（SkillTypeInfo.cs）

工作类型实例：`rwbpType`（R/W/B/P/N）、等级、名称/文案 tag。分配工作即构造并传给 OnWorkAllocated。

## FeelingStateCubeBounds（FeelingStateCubeBounds.cs）

- `int[] upperBounds`——**[差的上限, 良的上限, 总产能上限]**（长度 3，已验证）
- `CalculateFeelingState(int energyCubeNum)`：`≤upperBounds[0]→BAD`，`≤upperBounds[1]→NORM`，否则 `GOOD`（L6-24）
- `GetLastBound()` = `upperBounds[last]` = 工作总 tick 数 maxCubeCount
- 心情驱动逆卡巴拉：BAD 附近触发计数器减少（各异想体 CreatureBase 逻辑）

## mod 侧使用处

- `Confidence.cs`：n、p、伤害分布三要素全部来自本文件语义
- `Command/RepeatWorkCommand`：连续工作循环（每轮重新构造 UseSkill 流程）
- `Angela.Creature.*` 警告文案中"工作结果为良/优"即 feelingState 相关
