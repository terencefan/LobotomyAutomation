# Buf（单位缓冲系统）

路径均相对 `~/workspace/lobotomy-decompiled/Assembly-CSharp/`。

## UnitBuf 体系（UnitBuf.cs 基类）

- 挂在 `UnitModel` 上，经 `GetUnitBufList()` / `GetUnitBufByType(type)` 访问（**这两个方法是 BaseMod 补丁版 Assembly-CSharp 注入/暴露的**，游戏原版没有——lib/ 的 DLL 必须是补丁版的原因）
- 关键虚方法：`OnTakeDamage(UnitModel attacker, ref DamageInfo damage)`——伤害管线第 5 环消费（见 damage.md）；`OnFixedUpdate` 等生命周期
- 合并/叠加策略按子类各自实现

## BarrierBuf（护盾）

- 私有字段：`_rwbpType`（护盾类型）、`_barrierValue`（剩余值）——**AutoInority 用反射读**（`Extentions/AgentModelExtensions.cs` `GetBarrierValue`，静态缓存 FieldInfo，已验证可行）
- `UseBarrier(...)` **有副作用**（扣盾）——只读查询绝不能调它
- 类型语义：R/W/B/P 对应伤害类型；**A = 通用盾**（挡所有类型），GetBarrierValue 需把 A 型并入每种类型总量
- 来源：Talowland 等加盾异想体/饰品（未完整枚举）

## AutoInority 侧 buf

- **AutomatonBuf**（mod 类，挂 `agent.AddUnitBuf`）——标记"该员工正被 Automaton 管理"，配合 `ClearAutomationBuf()` 清除；注册/移除见 `Automaton.Register/Remove`
- `GetBarrierValue(this AgentModel, RwbpType)`：遍历 bufList，反射取 `_rwbpType/_barrierValue`，A 型计入所有类型——用于 CheckSurvive 的护盾池（PR #70）
