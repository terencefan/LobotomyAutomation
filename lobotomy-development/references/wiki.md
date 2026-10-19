# Wiki（中文资料源与术语对照）

外部中文基准：**脑叶公司 Wiki** `https://lobotomycorp.fandom.com/zh/`（用户指定，2026-10-18）。

## 权威规则（冲突裁决）

1. **中文译名 / 文案表述以 wiki 为准**
2. **数值 / 机制 / API 签名以反编译代码为准**（references 其余各文件）
3. 两者冲突 → 代码赢，并把差异记到本文件"已知差异"节；译名差异记到 `creatures-zh.md` 的 wiki 列

## 访问通道（2026-10-19 本机网络实测，按优先级）

1. **WebSearch（当前实测可用）**：claude 侧搜索，不走本机网络。`allowed_domains: ["lobotomycorporation.wiki.gg"]`（wiki.gg 镜像比 fandom 全），换多组关键词挖页面内容摘要，拼凑表格化事实
2. **WebFetch 不可用**：fandom 与 wiki.gg 域名均被 claude.ai 侧域名安全检查拒（"Unable to verify if domain ... is safe to fetch"）
3. **本机直连 fandom 不可用（网络层）**：SSL 被 MITM（curl exit 60）；`-k` 跳过校验后 HTTP 200 但 body 为空（响应体被网络层过滤）；环境变量里的本地代理 `127.0.0.1:7890` 未运行。换网络环境时重测
4. **fandom API（备用，原环境可用）**：`https://lobotomycorp.fandom.com/zh/api.php?action=query&format=json&titles=<名>&redirects=1`（批量用 `|` 连接，每批 ≤50）；全文解析 `action=parse&page=<名>`；搜索 `list=search&srsearch=`；前缀列举 `list=allpages&apprefix=`；**必须带 User-Agent**（python urllib 默认 UA 会 403，curl 自带 UA 可用）；串行 + sleep(0.3) 限速
- 页面 HTML（备用）：`curl https://lobotomycorp.fandom.com/zh/wiki/<页面名>?variant=zh`
- 页面名规则：**异想体页 = `<官方编号> <中文名>`**（如 `O-05-61 破裂盔甲`），直接输名字会命中 redirect；考验页 = `<颜色时段> <副标题>`（如 `琥珀色的黎明 新鲜的食物`）；URL 用 urllib.parse.quote 编码

## 异想体页面结构

编号与风险等级 / 伤害抗性表 / 工作偏好表 / 管理须知 / **观察记录（=场景描述，剧情与解法线索）** / 出逃信息。机制内容散见于相关页面（无独立"工作""伤害"页），优先读 `异想体` 总页与 `考验`/`抑制核心`/`ALEPH` 页。

## 术语对照表

### 属性与工作类型（代码已验证：SkillTypeInfo.rwbpType = (RwbpType)id，Skills_cn.xml id1-4）

| RwbpType | 工作类型 | 属性 | 简中 | 繁中（cn_tr XML） |
|---|---|---|---|---|
| R | Instinct | Fortitude | 本能 / 勇气 | 本能 / 勇氣 |
| W | Insight | Prudence | 洞察 / 谨慎 | 洞察 / 謹慎 |
| B | Attachment | Temperance | 沟通 / 自律 | 溝通 / 自律 |
| P | Repression | Justice | 压迫 / 正义 | 壓迫 / 正義 |

特殊技能 id：5 特殊请求、6 赎罪、7 演奏（→W）（Skills_cn.xml）。

### 等级与伤害

- 风险等级：ZAYIN < TETH < HE < WAW < ALEPH（RiskLevel 枚举序；wiki 有 `ALEPH` 独立页）
- 伤害色：R 红（HP）/ W 白（精神）/ B 黑（双扣）/ P 蓝（灵魂 %maxHp）——wiki 用"红伤/白伤/黑伤/蓝伤"表述

### 系统名词（wiki 页面核验：考验 ✓ 抑制核心 ✓ ALEPH ✓；其余以游戏 XML/代码为准）

逆卡巴拉计数器（qliphothCounter）、能量、E.G.O（装备/礼物 gift）、考验（Ordeal）、抑制核心（核心抑制，SefiraBoss）、职员（wiki 对 worker/agent 的称呼，`员工`会 redirect 到`职员`）、观察（Observe）、心情状态（feelingState：优/良/普通/差）。

### 异想体编号规则

前缀是 wiki 编号的组成部分（旧版分类遗留），**与游戏风险等级不对应**：O（偏低位）/T/F/W/A/D（衍生：WonderLab/Limbus 联动）；格式 `<前缀>-<部门两位>-<序号>`，变体加后缀 `-W/-H/-T/-Z`。**游戏 id（100xxx/200xxx/300xxx）与 wiki 编号无规律映射，对照查 `creatures-zh.md`**；前缀与实际等级大量不符（樱下墓 O-04-100=TETH、蕾蒂希娅 O-01-67=HE、别碰我 O-05-47=ZAYIN、爱娜温 T-04-53=WAW），风险等级以 `creatures-zh.md` 等级列（游戏 `*_stat.txt` riskLevel）为准。200xxx=考验，300xxx=WonderLab 衍生。

## 已知差异（游戏名 vs wiki 名，全表见 creatures-zh.md）

- 泰迪熊 → 快乐泰迪（T-04-06）；大蘑菇 → 小王子（O-04-66）；魔法少女 → 憎恶女王（O-01-04，wiki 用最终形态命名）
- 高鸟 → 审判鸟；小鸟 → 惩戒鸟；Price of Silence → 沉默的代价
- 游戏繁中翻译质量差（考验类残留韩文 녹빛 여명、绿色/紫色错乱）——繁中表述不作为基准
