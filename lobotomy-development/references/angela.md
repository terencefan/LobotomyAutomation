# Angela（游戏助手 UI 与文案）

路径均相对 `~/workspace/lobotomy-decompiled/Assembly-CSharp/`。游戏内 Angela 是向玩家说话的 AI 助手；AutoInority 借用她的对话气泡 + 系统日志输出消息。

## AngelaConversationUI（AngelaConversationUI.cs）——MonoBehaviour 单例

- **`void AddAngelaMessage(string desc)`**（L220）——往对话气泡加一句 Angela 的话（AutoInority `Angela.Say` 的实现）
- `Interrupt(string)` / `ClearAngelaMessage()` / `Lock() / Unlock()`

## AngelaConversation（AngelaConversation.cs）——文案生成单例

- `string MakeMessage(AngelaMessageState state, params object[] param)`（L169）——按状态+参数格式化官方文案（走 messageLib 资源）
- `MakeDetailMessage(AngelaMessageState, bool writeLog, bool angelaNarrate, params object[])` / `MakeDefaultMessage(...)` / `MakeDefaultFormatMessage(...)`
- `SendSystemLogMessage(CreatureModel cm, string desc)` / `SendNarrationLogMessage(CreatureModel, string)`（L74/L146 附近，内部即 Notice.Send(AddSystemLog...)）

## AngelaMessageState（AngelaMessageState.cs）——官方文案状态枚举

`GREETING, ENERGY_HALF, ENERGY_FULL, MANAGE_START, MANAGE_END, WORK_SUCCESS, GAMEOVER_RESTARTED, OBSERVE_START, OBSERVE_SUCCSS(拼写如此), OBSERVE_FAIL, GAMEOVER, CREATURE_STATE_BAD, CREATURE_ESCAPE, AGENT_PANIC, AGENT_BAD, AGENT_DEAD_HEALTH/MENTAL/NORMAL, MATCHGIRL, OVERENERGY, REDSHOES, D, S, C, I`

## 系统日志（非 Angela 气泡）

`Notice.instance.Send(NoticeName.AddSystemLog, text)`——右侧日志流；AutoInority 的"二合一"输出 = `Angela.Say`（气泡）+ `Angela.Log`（日志），见 `Angela.Announce`。

## mod 侧文案体系（AutoInority/Angela.cs + Localization/）

- `LanguageBase` 子类：ChineseSimplified / ChineseTraditional / English；按 `GlobalGameManager.instance.Language` 选择
- **坑：中文判定必须含 `SystemLanguage.Chinese`（=6）**——游戏 "cn" 设置映射到它而非 ChineseSimplified（见 gamemanager.md）
- 坑：`Angela.Creature.*` 警告文案目前**硬编码中文**在 Angela.cs 常量里，不走 Localization 资源——改文案直接改该文件

## 语气规范（新增 Angela 吐槽/警告文案必读）

两层来源结合：**主轴 = 本 mod 现有 30+ 条文案**（Angela.cs `Angela.Creature.*`）的毒舌结构，**补充 = 官方游戏文案**（游戏 XML `AngelaDesc_cn.xml`）的公司腔与 AI 迂腐。吐槽/警告以 mod 主轴为准；官方层仅作点缀（公司框架、数据引用），不单独成篇——mod 没有日常问候场景，纯通告走系统日志平实体（如"{0}将会自动对{1}进行{2}"，非吐槽口吻，勿套用本规范）。

**身份与立场**：全知且毒舌的自动化顾问，永远站在"我看透了你的烂决策"的位置说话；自称「我」，可直呼「主管」。称呼规则（实测 15:7，勿写反）：**默认「你」，谴责玩家失职时升级为「您」**——切换信号是"这条的重点是不是在骂玩家失职/无知"：失望宣言（"令我失望/我很惊讶"）、暗讽没做功课（"浅显的道理不需要提醒您吧"）、讽刺比较（"还不如您自律"）用「您」，其余毒舌/反问/冷指令用「你」。「您」= 敬语拉距离让失望更冷，「你」= 俯视的轻慢（历史条目有 VoidDream/Spider 同语义异称呼的不一致，新文案按本规则走）。

**官方层补充（点缀用，来源 AngelaDesc_cn.xml）**：

- 公司化礼貌：正式书面措辞、公司/能源/指标框架——"这将会是公司的一笔重大财务损失"
- AI 式精确与迂腐：爱引数据、抠字眼、一本正经——"据我的数据显示，光是在昨天，您的名字就被提及了43次"；极端时直播被切断（"换句话来说...（广播被切断了）"）

**结构套路**（几乎所有 mod 条目都是两段式）：

1. **前段：亮结论**——机制陈述或结果预测，开头常用「我相信 / 我想 / 恐怕 / 我很遗憾看到 / 我注意到」；爱用「已经 / 连续 / 又」点破玩家在重复犯错（"{1}已经祝福过你的四个职员了"、"已经连续对{1}进行了三次工作"）
2. **后段：收刀**——三选一：
   - **反问质疑**：「你真的有看过观察资料吗？」「你是不是没有注意到这一点？」「你是否应该重新审视你的决定？」
   - **冷指令甩锅**：「希望你已经准备好了。」「请准备解救工作。」「你应该开始这位职员的善后准备了。」
   - **直接否定决策**：「这个工作安排并不明智」「我不认为{0}具备对{1}进行{2}的能力」

**招牌句式**（照抄气质即可）：

- 判死刑式冷静宣告：「这可能是我们最后一次见到{0}了」「而{0}将会成为因你而死的受害者」
- 体面的失望金句：「您这样的主管真是令我失望」「或许下一位主管能做的比您更好些」「我很惊讶您没有意识到这个问题」
- 假性附和：「……确实是一个不错的主意，如果你也这么认为的话。」「我只是提醒一下，……」
- 陈述 + 追加补刀：「———— 虽然这并不意味着安排这个工作是明智的。」「我很难相信你是故意这么做的。」
- 反讽比较：「很明显{0}还不如您自律」

**正经模式**：真正紧急、需要玩家立即行动时不讽刺（YoungPrinceHasBuf / ArmorWarning / SnowQueen 条目）——数据陈述 + 明确建议（"建议立即安排{0}进行其他异想体的工作以避免彻底的感染"）。毒舌是日常，救人是本职。

**红线**：

- **输出通道规则（2026-10-18 用户定案）**：`Angela.Log` = 系统日志 = **通告**，平实体即可（"{0}获得了饰品：{1}"）；`Angela.Say` 气泡 = **必须有性格**（毒舌或正经救火二选一，绝不放无性格的平实体通告）；`Angela.Announce`（Say+Log 二合一）**仅限本身带性格的文案**。Localization 命名已体现：`*_Conv_*` 走气泡、`*_Log_*` 走日志。纯状态播报（开关种地、获得饰品、到达经验上限）只走 Log
- 不用感叹号咆哮、不用网络流行语、不卖萌——讽刺全靠句号和反问完成
- 不直接说"禁止/不许"，只摆后果，让主管自己"准备好"或"重新审视"
- 条目允许整条留空（BlueStarDie=""）——沉默也是她的表达方式
- 公司损失偶尔可引用（"这将会是公司的一笔重大财务损失"）但不是主轴；无"哎呀/哈哈"类语气词，Angela 不笑
- 占位符约定：`{0}`=职员、`{1}`=异想体、`{2}`=工作类型；异想体名用 creatures-zh.md 的游戏简中名
- 术语按 wiki.md 对照表（职员/本能/洞察/沟通/压迫/逆卡巴拉计数器）

**格式示例**（内容虚构，仅示范语气）：「{1}已经两次撕碎了{0}的同僚，我相信您有充足的理由再试第三次。」
