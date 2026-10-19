# CreatureBase（异想体行为脚本基类）

`CreatureBase.cs`——**非 MonoBehaviour**，纯脚本类，实例挂在 `CreatureModel.script`。游戏所有异想体脚本（如 HealthBracelet）继承它。行号已对照反编译源码验证。

## 工作相关钩子

| 签名 | 行号 | 语义 |
|---|---|---|
| `virtual void OnWorkAllocated(SkillTypeInfo skill, AgentModel agent)` | L1073 | 工作分配时 |
| `virtual void OnSkillTickUpdate(UseSkill skill)` | — | 每 tick |
| `virtual void OnSkillSuccessWorkTick(UseSkill skill)` / `OnSkillFailWorkTick(UseSkill skill)` | L394 / L390 | 单 tick 成功/失败结算 |
| `virtual bool isAttackInWorkProcess()` | L539 | **工作失败是否造成伤害**（默认 true；AutoInority 置信度模型的关键输入） |
| `virtual void OnAttackInWorkProcess(UseSkill skill)` | L544 | 工作中造成伤害时 |
| `virtual bool ForcelySuccess(UseSkill skill)` / `ForcelyFail(UseSkill skill)` | L1225 / L1230 | 强制成功/失败（默认 false，特殊异想体覆写） |
| `virtual float TranformWorkProb(float originWorkProb)` | L426 | 改写成功率（注意拼写 **Tranform**，非 Transform） |
| `virtual int OnBonusWorkProb()` | L416 | 额外成功率加成 |
| `virtual float GetDamageMultiplierInWork(UseSkill skill)` | L421 | 工作伤害倍率 |
| `virtual void OnEnterRoom(UseSkill)` / `OnFinishWork(UseSkill)` / `OnReleaseWork(UseSkill)` | — | 进入房间/工作结束/释放 |
| `virtual void OnAllocatedWork(AgentModel)` / `OnReleaseWorkAllocated()` | — | 分配/释放分配 |
| `virtual void OnWorkCoolTimeEnd(CreatureFeelingState oldState)` | — | 心情冷却结束 |

## 出逃/镇压/生命周期

- `virtual void Escape()` / `UniqueEscape()`——出逃（OrdealCreatureModel 中被禁用，见 ordeal.md）
- `virtual void OnSuppressed()` / `bool IsSuppressable()` / `bool IsAutoSuppressable()`——镇压
- `virtual void OnInit()` / `OnStageStart()` / `OnStageRelease()` / `OnFixedUpdate(CreatureModel)`
- `virtual void OnObserveLevelChanged()`——观察度提升
- `virtual bool CanEnterRoom()` / `bool IsWorkable()` / `SkillTypeInfo GetSpecialSkill()`
- `virtual bool OnWorkerPanic(WorkerModel worker)`（L1035，默认 false）——员工恐慌时
- 存档：`GetSaveData` / `LoadData`

## mod 侧使用处

- `Creature/BaseCreatureExt.cs`（AutoInority 全部 75 个扩展的公共基类）覆写上述钩子提供警告/拦截；`CheckSurvive()` 建模直接消费 `isAttackInWorkProcess()` 与伤害数据
- 新增异想体支持 = 写 `XxxExt : BaseCreatureExt` 并注册到扩展映射（见 autoinority.md）
