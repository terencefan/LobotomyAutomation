# Damage（伤害系统）

路径均相对 `~/workspace/lobotomy-decompiled/Assembly-CSharp/`。

## DamageInfo（DamageInfo.cs，已验证）

- 字段：`RwbpType type`、`float min`、`float max`（实际伤害在 [min,max] 均匀取值）、`SoundInfo/EffectInfo` 系列、`string param`
- `static DamageInfo zero`、构造 `DamageInfo(type, damage)` / `DamageInfo(type, int min, int max)`
- `DamageInfo Copy()`；`operator *(DamageInfo, float)`（乘区用，min/max 同乘）
- `float GetDamage()`；`GetDamageWithDefenseInfo(DefenseInfo)`——应用抗性

## RwbpType（RwbpType.cs，全枚举已验证）

`N, R, W, B, P, A`——N=无/红(物理通用)，R=红(HP)，W=白(精神)，B=黑(双扣)，P=蓝(灵魂，%maxHp)，**A=通用护盾类型**（只用于 BarrierBuf，见 buf.md）。

## 伤害管线（WorkerModel.TakeDamage，WorkerModel.cs L587-613，已验证）

顺序严格如下，每环都能改写 DamageInfo：

1. **armor.OnTakeDamage**（护甲脚本，ref 改写）
2. **weapon.OnTakeDamage**（武器脚本）
3. **gifts.OnTakeDamage**（饰品脚本）
4. **EGO 等级乘区**：`GetDmgMultiplierByEgoLevel(攻击等级, 防御等级)`（UnitModel.cs L798，精确表 0.4~2.0 见 unit.md）
5. **buf 乘区**：`GetBufDamageMultiplier(attacker, damage)`（UnitModel.cs L818，遍历 UnitBuf）
6. **抗性**：`GetDamageWithDefenseInfo(DefenseInfo)`（DefenseInfo.GetMultiplier 按类型查 resist 表）

## 各类型特殊语义（WorkerModel.cs，已验证）

- **B（黑）**：约 L668 起——**HP 和 mental 双扣满值**（建模时两个池都要算）
- **P（蓝）**：约 L711 起——`maxHp × value/100` 百分比伤害
- **R/W**：常规扣 hp / mental
- mental 归零 → 恐慌（`OnAgentPanic` 事件，见 notice.md）

## DefenseInfo（DefenseInfo.cs）

抗性数据（按 RwbpType 的 resist 等级表）；`GetMultiplier(RwbpType)` 换算乘数（未逐项验证）。

## AutoInority 使用处

- `Creature/BaseCreatureExt.cs` `CheckSurvive()`：按管线逆推可承受伤害——装备白名单修正（对应第 1-3 环）+ EgoLevel 乘区（第 4 环，`agent.level` vs `GetRiskLevel()`）+ 护盾池（第 5 环前的独立建模）
- B 池建模：`min(hp, MentalFix(mental))`（MentalFix 是 **mod 侧** BaseCreatureExt 虚方法，默认恒等，CensoredExt 覆写为 0.4×）
