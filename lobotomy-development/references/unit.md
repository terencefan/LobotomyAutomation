# Unit（单位模型链）

路径均相对 `~/workspace/lobotomy-decompiled/Assembly-CSharp/`（平铺结构，类名即文件名）。

## 继承链

`UnitModel`（UnitModel.cs）→ `WorkerModel`（WorkerModel.cs）→ `AgentModel`（AgentModel.cs）；
`OfficerModel`（OfficerModel.cs）: WorkerModel 是 Sefira 主管 AI 单位（未深入验证）。

## UnitModel 基类

- `hp / maxHp / mental / maxMental`（float）——置信度建模的基础量
- `Equipment`（→ UnitEquipSpace，见 equipment.md；UnitModel.cs L70）
- buf 接口：`GetUnitBufList()` / `GetUnitBufByType()`（见 buf.md）
- `static float GetDmgMultiplierByEgoLevel(int attackLevel, int defenseLevel)`（L798，已验证）——攻防等级差乘区，精确表：≤-4→0.4，-3→0.6，-2→0.7，-1→0.8，0/1→1.0，2→1.2，3→1.5，≥4→2.0
- `virtual float GetBufDamageMultiplier(UnitModel attacker, DamageInfo damage)`（L818）
- `TakeDamage(DamageInfo)` 逻辑在 WorkerModel 覆写（见 damage.md）
- 移动/定位：`GetCurrentViewPosition()`、`movableNode`

## 四维属性 ↔ 工作类型（AgentModel，已验证）

| RwbpType | 属性 | 中文 | 快捷属性 |
|---|---|---|---|
| R | fortitudeLevel | 勇气 | `Rstat` |
| W | prudenceLevel | 谨慎 | `Wstat` |
| B | temperanceLevel | 自律 | `Bstat` |
| P | justiceLevel | 正义 | `Pstat` |

- 等级换算：`fortitudeLevel => CalculateStatLevel(fortitudeStat)`，1..5 级（AgentModel.cs L161-185）
- 成功率映射：`CreatureModel.GetWorkSuccessProb(actor, skill)`（CreatureModel.cs L1298，已验证）：N 型恒 1.0；R/W/B/P 分别查 `metaInfo.workProbTable.GetWorkProb(rwbpType, 对应属性等级)`
- Hod（YESOD? 未验证具体 Sefira）激活时有全局属性加成分支（AgentModel.cs L140）

## WorkerModel / AgentModel 关键成员

- `TakeDamage(DamageInfo)`（WorkerModel.cs L587 起）——伤害管线入口，见 damage.md
- `mental` 归零 → 恐慌；`CurrentPanicAction`（Automaton.AgentOnFixedUpdate 检查）
- `unconAction`（Uncontrollable_RedShoesAttract / Yggdrasil / Baku / YoungPrince / Sakura 等）——`OnClick()` 手动解除，AutoInority `Automaton.HandleUncontrollable()`
- AgentModel：`ClearAutomationBuf()/AddUnitBuf()`（mod 的 AutomatonBuf 挂点）、`Equipment.gifts`（礼物推荐 forGift）、`IsRegionLocked(giftMetaInfo)`
- 等级/经验结算（`levelUpStats`）未深入验证

## mod 侧使用处

- Confidence.cs：hp/mental/maxHp/maxMental + 属性等级 → 生存概率
- BaseCreatureExt.cs：CheckSurvive 读装备与护盾（unit.md 的 Equipment/gifts）
