# Notice（全局事件总线）

路径均相对 `~/workspace/lobotomy-decompiled/Assembly-CSharp/`。

## Notice（Notice.cs）——字符串事件总线，单例 `Notice.instance`

**坑：没有 `Publish` 方法**（已确认）。API 全集：

- `void Send(string notice, params object[] param)`——发布
- `int Observe(string notice, NoticeReciever observer)`——订阅，返回 noticeId
- `void Observe(string notice, IObserver observer)`
- `void Remove(string notice, int noticeId)` / `void Remove(string notice, IObserver observer)`
- 委托 `NoticeReciever(params object[] param)`（NoticeReciever.cs）；接口 `IObserver.OnNotice(string, params object[])`（IObserver.cs）

## NoticeName（NoticeName.cs）——全部事件名常量

`public static string` 字段，静态构造器赋同名串。常用（已核对，注意**字段名与实际串拼写不一致**的坑）：

| 分类 | 事件 |
|---|---|
| 工作 | OnWorkStart / OnProcessWorkTick / OnWorkCoolTimeEnd / OnReleaseWork / WorkEndReport / OnMaxObserveSuccess |
| 镇压 | OnCreatureSuppressed / CreatureSuppressCancel / OnCommandSuppress / OnCreaturePhysicsAttack·MentalAttack·ComplexAttack / WorkerAttackEnd |
| 出逃/考验 | EscapeCreature / OnEscape / OnOrdealActivated / OnOrdealStarted / OrdealEnd / OnEmergencyLevelChanged |
| 日期 | UpdateDay / OnNextDay / OnStageStart / OnStageEnd / OnFailStage |
| 员工 | OnAgentDead / OnAgentPanic / OnAgentPanicReturn / OnOfficerDie / OnOfficerPanic / AddNewAgent / RemoveAgent |
| 系统 | AddSystemLog / AddPlayerLog / AddNarrationLog / CreatureObserveLevelAdded / OnQliphothOverloadLevelChanged / OnGetEGOgift |

- 拼写坑：`UpdateAgentState_`（串带下划线）、`OnIsloateOverloaded`（Isloate）、字段 `PassageWhitle`、`LanaguageChange`（gamemanager.md）
- 动态事件名：`NoticeName.MakeName(string noticeName, params string[] param)`——下划线拼接

## AddSystemLog 调用形态（系统日志）

`Notice.instance.Send(NoticeName.AddSystemLog, cm, desc)`（带生物上下文）或 `Send(NoticeName.AddSystemLog, text)`（纯文本）——AutoInority 的 `Angela.Log` 即单参形态。

## mod 侧使用处

- `AutoInority/Angela.cs`：`Log()` = Send(AddSystemLog)；Harmony 补丁挂 OnAgentPanic 等事件驱动 Automaton
- TodayOrdeal：订阅 Ordeal 事件推送当日考验
