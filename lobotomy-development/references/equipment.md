# Equipment（装备系统）

路径均相对 `~/workspace/lobotomy-decompiled/Assembly-CSharp/`。

**坑：游戏里没有 `EquipmentInventory` 类。** 实际容器是 **UnitEquipSpace**（已全树 grep 确认）。

## UnitEquipSpace（UnitEquipSpace.cs）

经 `UnitModel.Equipment` 暴露（UnitModel.cs L70，已验证）：

- `WeaponModel weapon` / `ArmorModel armor`
- `UnitEGOgiftSpace gifts = new UnitEGOgiftSpace()`
- `CreatureModel kitCreature`——装备中的工具箱生物（见 kit.md）

## UnitEGOgiftSpace（UnitEGOgiftSpace.cs）

- `List<EGOgiftModel> replacedGifts`、`List<EGOgiftModel> addedGifts`——替换型/附加型饰品集合**在这里**，不在 EquipmentModel 上（已验证）
- EGOgiftModel : EquipmentModel——饰品；槽位/区域锁逻辑由 mod 侧 `IsRegionLocked` 处理

## EquipmentModel（EquipmentModel.cs，装备基类）

- `EquipmentTypeInfo metaInfo`、`long instanceId`、`EquipmentScriptBase script = new EquipmentScriptBase()`、`owner => UnitModel`
- 虚方法：
  - `OnEquip(UnitModel)` / `OnRelease()`
  - **`OnTakeDamage(UnitModel, ref DamageInfo)`**——伤害管线第二环，`ref` 可直接改写伤害（见 damage.md 管线顺序）
  - `GetBonus(UnitModel)` / `CheckRequire(UnitModel)`
- 子类：`WeaponModel`（WeaponModel.cs）、`ArmorModel`（ArmorModel.cs），均 : EquipmentModel
- 脚本基类：`EquipmentScriptBase`（EquipmentScriptBase.cs）——每件装备的自定义逻辑挂 script，mod 按 script 类型分发（AutoInority 的装备白名单即此模式）

## AutoInority 使用处

- `Creature/BaseCreatureExt.cs` `ApplyEquipmentDamageModifiers`：遍历 weapon/armor/gifts，按 **script 类型白名单**应用修正（DeathAngelArmor 且非持 400015、NamelessFetusGift；有状态装备跳过）
- `Extentions/AgentModelExtensions.cs` `GetBarrierValue`：反射读 BarrierBuf（见 buf.md）
- Angela.ArmorWarning/ArmorKill：盔甲特殊交互的警告文案
