# Creature（异想体数据面）

路径均相对 `~/workspace/lobotomy-decompiled/Assembly-CSharp/`。

## CreatureModel（CreatureModel.cs）

运行时异想体模型，数据面（脚本行为在 CreatureBase，见 creature-base.md）：

- `CreatureTypeInfo metaInfo`——静态数据（workProbTable、riskLevel、observeCost、creatureKitType 等）
- `CreatureBase script = new CreatureBase()`——行为脚本实例挂这里
- `int qliphothCounter`（只读属性 `_qliphothCounter`）——逆卡巴拉计数器；`ResetQliphothCounter()` / `SubQliphothCounter()`
- `bool isOverloaded`——能量过载（ qliphoth 峰值超载，Automaton.InEmergency 判定条件之一）
- `CreatureFeelingState feelingState`（枚举：NONE/GOOD/NORM/BAD）+ `feelingStateRemainTime`——心情由 FeelingStateCubeBounds 按产能算（见 work.md）
- `CreatureObserveInfoModel observeInfo`——观察度；`isPrevMaxObserve()` 判断次高观察
- `CreatureState state { get; set; }`（CreatureState.cs）：WAIT / WAIT_COOLDOWN / ESCAPE / WORKING / WORKING_SCENE / WORKING_AUTO / SUPPRESSED / SUPPRESSED_RETURN / OBSERVE
- `override int GetRiskLevel()`→ 返回 `(int)metaInfo.GetRiskLevel() + 1` 即 **1..5**；枚举 RiskLevel（RiskLevel.cs）：ZAYIN/TETH/HE/WAW/ALEPH（0..4）
- `kitEquipOwner`（AgentModel）——装备此 kit 的职员（kit 专用）
- `float GetWorkSuccessProb(AgentModel actor, SkillTypeInfo skill)`（L1298，已验证）
- `float GetCubeSpeed()`——产能速度
- 出逃相关：`Escape()` / `UniqueEscape()`（CreatureBase 虚方法转发，见 creature-base.md）

## CreatureTypeInfo（CreatureTypeInfo.cs）

静态模板：`workProbTable`（各 RwbpType 各属性等级的成功率表）、`GetRiskLevel()`、观察等级/消耗、`creatureKitType`、特殊技能表 `specialSkillTable`（未逐项验证）。

## AutoInority 使用处

- `Automaton.Main()`：`IsAvailable()`、`isOverloaded`（→ ManageKitCommand/ManageCreatureCommand）、`state == CreatureState.ESCAPE`（→ SuppressCommand）
- `Command/ManageCreatureCommand`：工作分配决策读 metaInfo + feelingState
- 各 `Creature/*Ext.cs`（75 个扩展类）读 qliphothCounter/feelingState 做警告
