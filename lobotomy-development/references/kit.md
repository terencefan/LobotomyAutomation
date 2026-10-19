# Kit（工具箱类异想体）

路径均相对 `~/workspace/lobotomy-decompiled/Assembly-CSharp/`。

**坑：不存在 `BaseKitCreature` 类**（已全树 grep 确认）。kit 生物的脚本基类仍是 CreatureBase，工具箱专属逻辑挂在嵌套类 **`CreatureBase.KitEquipEventListener`** 上。

## 判定与类型

- `CreatureWorkType`（CreatureWorkType.cs）：`NORMAL / KIT`——metaInfo 上的工作类型
- `CreatureKitType`（CreatureKitType.cs）：`NONE / EQUIP / CHANNEL / ONESHOT`——metaInfo.creatureKitType
- AutoInority 侧 `CreatureModel.IsKit()` 扩展方法（按 CreatureWorkType.KIT 判定）

## KitEquipEventListener（CreatureBase.cs，嵌套类）

- 实例字段：`public KitEquipEventListener kitEvent = new KitEquipEventListener();`（CreatureBase.cs L301）
- 虚方法：`OnViewInit(CreatureUnit)` / `OnUseKit(AgentModel)` / `OnEnterRoom(AgentModel, UseSkill)` / `OnFixedUpdateInKitEquip(AgentModel)` / `OnAttack(AgentModel, UnitModel)` / `OnTakeDamagePhysical/OnTakeDamageMental(WorkerModel, float)` / `OnCommandReleaseKitEquip/OnReleaseKitEquip(AgentModel, ...)`
- 调用形态：`agent.Equipment.kitCreature.script.kitEvent.OnEnterRoom(agent, this)`（UseSkill.cs L164，已验证）
- 范例脚本：`HealthBracelet.cs`——构造里 `kitEvent = new HealthBraceletKit(this);`

## 观察度

`KitCreatureObserveInfo`（KitCreatureObserveInfo.cs）：`totalKitUseCount / totalKitUseTime` 对照 observeCost——kit 用使用次数/时长而非工作次数计观察（未逐项验证）。

## AutoInority 使用处

- `Automaton.Main()`：遍历 `IsKit()` 的生物，`isOverloaded` 时入队 `ManageKitCommand`，并每帧调 `kit.GetKitExtension().OnFixedUpdate()`（扩展体系见 autoinority.md）
- `Command/ManageKitCommand`：kit 的自动化管理（让职员装备/使用）
