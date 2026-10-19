# Ordeal（考验）

路径均相对 `~/workspace/lobotomy-decompiled/Assembly-CSharp/`。

## OrdealManager（OrdealManager.cs）——单例 `OrdealManager.instance`

- `OrdealCreatureModel[] GetOrdealCreatureList()`——当前所有考验单位
- `bool ActivateOrdeal(OrdealBase ordeal, bool remove = true)`
- `OrdealCreatureModel AddCreature(long metadataId, MapNode pos, OrdealBase ordealBase)`；mod 版 `AddCreature_Mod(LcIdLong, MapNode, OrdealBase)`
- `List<OrdealBase> GetActivatedOrdeals()` / `CheckOrdealContains(OrdealLevel, out OrdealBase)` / `OnOrdealEnd(OrdealBase, bool)`

## OrdealCreatureModel（OrdealCreatureModel.cs，: CreatureModel）

- **构造即 `state = CreatureState.ESCAPE`**，阵营 `EscapedCreature`
- **`Escape() / OnStageStart() / OnStageRelease()` 被 override 成只打 LogError**——考验单位不能也不需要"出逃"，它天生在场（已验证）
- 自带 `OnFixedUpdate()`（直调 script、命令队列、移动逻辑）
- `IsHostile(UnitModel)`：对 Worker 恒 true，对其他 OrdealCreature 恒 false——考验互不攻击
- 持有 `OrdealBase OrdealBase` + `SetOrdealBase(OrdealBase)`

## OrdealBase（OrdealBase.cs）

- `OrdealLevel level`；虚方法 `OnOrdealStart / OrdealEnd / FixedUpdate / IsStartable / GetRiskLevel(OrdealCreatureModel)`
- OrdealLevel（OrdealLevel.cs）——考验等级（AMBER/CRIMSON/GREEN/DAWN 类，未逐项验证）

## 事件

`OnOrdealActivated / OnOrdealStarted / OrdealEnd`（见 notice.md）；日期推进时由日历系统触发激活。

## AutoInority 使用处

- `Automaton.Main()`：`OrdealManager.instance.GetOrdealCreatureList()` 全部入队 `SuppressCommand`（考验无条件镇压）
- `Automaton.InEmergency`：考验存在 && 有过载收容物 → 紧急模式
- TodayOrdeal：开局读取当日考验类型并通知
