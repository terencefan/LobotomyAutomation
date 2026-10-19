# Creatures（异想体全量中文译名表）

**数据源（2026-10-18 生成）**：游戏本体本地化 `LobotomyCorp_Data/ExternalData/xml/Language/{cn,cn_tr}/creatures/*.xml`（简繁各 101/95 文件，游戏内显示名为准）；wiki 列经 `lobotomycorp.fandom.com/zh` api.php 全量核验（99/101 有页面，页面名带官方编号前缀）。
**等级列（2026-10-19 提取）**：以游戏运行时数据为准——`Managed/BaseMod/BaseCreatures/*_stat.txt` 的 `<riskLevel>`（1=ZAYIN / 2=TETH / 3=HE / 4=WAW / 5=ALEPH）；例外：终末鸟取 `ChildCreatures/BossBird_stat.txt`；v1.1 五件礼物异想体（别碰我/光之锤/逆行时钟/穷编剧的手册/柳橙树）stat 无此字段，取语言 xml 图鉴 `<riskLevel>`。**wiki 编码前缀 ≠ 风险等级**（樱下墓 O-04-100=TETH、蕾蒂希娅 O-01-67=HE、别碰我 O-05-47=ZAYIN、爱娜温 T-04-53=WAW），等级一律以本表为准。
**权威规则**：数值/机制以代码为准；**译名/文案表述以 wiki 为准**（冲突处本表"wiki 页面名"列已并记差异，如 泰迪熊→快乐泰迪、大蘑菇→小王子、魔法少女→憎恶女王）。

| script 类名 | 游戏 id | 等级（游戏 riskLevel，按 ZAYIN→ALEPH 分区） | 简体（游戏 cn） | 繁体（游戏 cn_tr） | wiki 页面名 |
|---|---|---|---|---|---|
| Onebadmanygood | 100009 | ZAYIN | 一罪与百善 | 一罪百善 | O-03-03 一罪与百善 |
| PlagueDoctor | 100014 | ZAYIN | 疫医 | 疫醫 | O-01-45 疫医 |
| LadyLookingAtWall | 100022 | ZAYIN | 面壁女 | 面壁的女士 | F-01-18 面壁女 |
| DontTouchMe | 100024 | ZAYIN | 别碰我 | 別碰我 | O-05-47 别碰我 |
| LightsHammer | 100025 | ZAYIN | Hammer of light（原文如此） | 熾光之鎚 | O-05-48-Z 光之锤 |
| Wellcheers | 100028 | ZAYIN | 韦尔奇乐牌汽水 | 威爾奇爾思 | F-05-52 韦尔奇乐牌汽水 |
| Bald | 100034 | ZAYIN | 你是个秃子... | 你禿了… | 秃头-真是-太棒啦! 你是个秃子... |
| Fairy | 100053 | ZAYIN | 精灵盛宴 | 小精靈的宴會 | F-04-83 精灵盛宴 |
| PinkCorps | 100064 | ZAYIN | 粉红「小分队」 | — | （wiki 无页面） |
| ResetMirror | 300007 | ZAYIN | 转性魔镜 | 改變之鏡 | O-09-81 转性魔镜 |
| IronMaiden | 300101 | ZAYIN | 我们可以改变一切 | 我們能改變一切 | T-09-85 我们可以改变一切 |
| YouMustHappy | 300105 | ZAYIN | 你必须要幸福 | 你一定會快樂 | T-09-94 你必须要幸福 |
| PromiseAndFaith | 300108 | ZAYIN | 古老的信念与承诺 | — | T-09-97 古老的信念与承诺 |
| Dummy | 100000 | TETH | 教学兔兔「Dummy」 | 基礎訓練用模型兔子 | O-00-00 教学兔兔「Dummy」 |
| Matchgirl | 100001 | TETH | 焦化少女 | 火柴女孩 | F-01-02 焦化少女 |
| 1.76mhz | 100007 | TETH | 1.76兆赫 | 1.76 兆赫 | T-06-27 1.76兆赫 |
| SpiderMom | 100012 | TETH | 蜘蛛巢 | 蜘蛛太太 | T-02-43 蜘蛛巢 |
| BeautyBeast | 100013 | TETH | 美女和野兽 | 美女與野獸 | F-02-44 美女和野兽 |
| StraitJacket | 100018 | TETH | 被遗弃的杀人魔 | 遭遺忘的殺人犯 | T-01-54 被遗弃的杀人魔 |
| SmallBird | 100020 | TETH | 小鸟 | 小小鳥 | O-02-56 惩戒鸟 |
| OldLady | 100021 | TETH | 老妇人 | 老婆婆 | O-01-12 老妇人 |
| BloodBath | 100027 | TETH | 血浴缸 | 血浴池 | T-05-51 血浴缸 |
| Cosmos | 100036 | TETH | 宇宙碎片 | 宇宙碎片 | O-03-60 宇宙碎片 |
| ArmorCreature | 100037 | TETH | 破裂盔甲 | 崩壞的盔甲 | O-05-61 破裂盔甲 |
| ShyThing | 100052 | TETH | 今天也很害羞 | 今天的害羞表情 | O-01-92 今天也很害羞 |
| Bunny | 100054 | TETH | 陆生鮟鱇 | 肉色夜燈 | O-04-84 陆生鮟鱇 |
| Sakura | 100059 | TETH | 樱下墓 | 櫻花樹之墓 | O-04-100 樱下墓 |
| Baku | 100060 | TETH | 空虚之梦 | 空洞夢境 | T-02-99 空虚之梦 |
| Ppodae | 100106 | TETH | 波迪 | — | D-02-107 波迪 |
| CircusDawn | 200005 | TETH | 血色的黎明 | 녹빛 여명 | 血色的黎明 开始欢呼吧！ |
| OutterGodDawn | 200009 | TETH | 紫罗兰的黎明 | 綠色黎明 | 紫罗兰的黎明 理解的果实 |
| BugDawn | 200013 | TETH | 琥珀色的黎明 | 綠色黎明 | 琥珀色的黎明 新鲜的食物 |
| DesireHeart | 300001 | TETH | 渴望之心 | 渴望的心 | T-09-77 渴望之心 |
| Theresia | 300003 | TETH | 特蕾西娅 | 特蕾希雅 | T-09-09 特蕾西娅 |
| ProphecyOfSkin | 300103 | TETH | 人皮启示录 | 肌膚的預言 | T-09-90 人皮启示录 |
| HealthBracelet | 300106 | TETH | 荧光手镯 | 發光的手鐲 | O-09-95 荧光手镯 |
| JusticeReceiver | 300107 | TETH | 行为矫正仪 | 行為矯正 | O-09-96 行为矫正仪 |
| Yang | 300109 | TETH | 阳 | 陽 | O-07-103 阳 |
| Happyteddy | 100002 | HE | 泰迪熊 | 泰迪熊 | T-04-06 快乐泰迪 |
| RedShoes | 100003 | HE | 红舞鞋 | 紅色舞鞋 | O-04-08 红舞鞋 |
| SingingMachine | 100006 | HE | 歌唱机 | 唱歌的機器 | O-05-30 歌唱机 |
| Helper | 100011 | HE | 小帮手 | 小幫手 | T-05-41 小帮手 |
| Rudolph | 100016 | HE | 雪橇鲁道夫 | 魯道塔 | F-02-49 雪橇鲁道夫 |
| NamelessFetus | 100017 | HE | 无名怪婴 | 無名的胎兒 | O-01-15 无名怪婴 |
| GalaxyBoy | 100031 | HE | 银河之子 | 銀河之子 | O-01-55 银河之子 |
| LittleWitch | 100041 | HE | 害羞的小孩 | 害羞的女孩 | O-01-67 蕾蒂希娅 |
| Butterfly | 100043 | HE | 亡蝶葬仪 | 葬禮亡蝶 | T-01-68 亡蝶葬仪 |
| LookAtMe | 100049 | HE | 幸灾乐祸 | 幸災樂禍 | O-05-76 幸灾乐祸 |
| Scarecrow | 100050 | HE | 索求智慧的稻草人 | 尋求智慧的稻草人 | F-01-87 索求智慧的稻草人 |
| Lumberjack | 100051 | HE | 热心的樵夫 | 熱心的樵夫 | F-05-32 热心的樵夫 |
| Porccu | 100057 | HE | 棘刺公交 | 荊棘魔女 | O-02-98 棘刺公交 |
| ScytheClock | 100101 | HE | Price of Silence（原文如此） | 沉默的代價 | O-05-65-H 沉默的代价 |
| SnowQueen | 100102 | HE | 冰雪女皇 | 冰雪女王 | F-01-37 冰雪女皇 |
| Freischutz | 100103 | HE | 魔弹射手 | 魔彈射手 | F-01-69 魔弹射手 |
| MachineDawn | 200001 | HE | 绿色的黎明 | 綠色黎明 | 绿色的黎明 疑问 |
| MachineNoon | 200002 | HE | The noon of green（原文如此） | The noon of green | 绿色的正午 理解的过程（推断，游戏文件未汉化） |
| CircusNoon | 200006 | HE | 血色的正午 | 녹빛 여명 | 血色的正午 汁水大合唱 |
| ResearcherNote | 300002 | HE | 癫狂研究员的笔记本 | 瘋狂研究員的筆記 | T-09-78 癫狂研究员的笔记本 |
| BigTreeSap | 300005 | HE | 巨树汁液 | 巨樹的汁液 | T-09-80 巨树汁液 |
| Shelter | 300006 | HE | 3月27日的避难所 | 三月二十七日的避難所 | T-09-82 3月27日的避难所 |
| OtherWorldPortrait | 300104 | HE | 异世的肖像 | 異世界的肖像畫 | O-09-91 异世的肖像 |
| OutterGodNoon |  | HE | 紫罗兰的正午 | 녹빛 여명 | 紫罗兰的正午 请给我们爱！！！ |
| MagicalGirl | 100004 | WAW | 魔法少女 | 魔法少女 | O-01-04 憎恶女王 |
| Bigbird | 100008 | WAW | 大鸟 | 大鳥 | O-02-40 大鸟 |
| OrangeTree | 100010 | WAW | My Sweet Orange Tree（原文如此） | 我親愛的柳橙樹 | O-01-04-W 我的酸酸橙子树 |
| SnowWhite | 100023 | WAW | 白雪公主的苹果 | 白雪公主的蘋果 | F-04-42 白雪公主的苹果 |
| QueenBee | 100026 | WAW | 蜂后 | 蜂后 | T-04-50 蜂后 |
| Alriune | 100029 | WAW | 爱娜温 | 艾露內 | T-04-53 爱娜温 |
| AuthorNote | 100030 | WAW | Poor Screenwriter’s Note（原文如此） | 悲慘編劇的筆記 | O-05-31-W 穷编剧的手册 |
| RedHood | 100032 | WAW | 小红帽雇佣兵 | 小紅帽雇傭兵 | F-01-57 小红帽雇佣兵 |
| BigBadWolf | 100033 | WAW | 又大又可能很坏的狼 | 又大又壞的狼...大概啦 | F-02-58 又大又可能很坏的狼 |
| LongBird | 100035 | WAW | 高鸟 | 長長鳥 | O-02-62 审判鸟 |
| MagicalGirl_2 | 100039 | WAW | 魔法少女 | 魔法少女 | O-01-04 憎恶女王 |
| YoungPrince | 100040 | WAW | 大蘑菇 | 大蘑菇 | O-04-66 小王子 |
| BloodyTree | 100044 | WAW | 穿刺乐园 | 滲透天堂 | O-04-72 穿刺乐园 |
| BlackSwan | 100045 | WAW | 黑天鹅之梦 | 黑天鵝的夢 | F-02-70 黑天鹅之梦 |
| ViscusSnake | 100046 | WAW | 裸巢 | 裸巢 | O-02-74 裸巢 |
| Shark | 100047 | WAW | 梦中的洋流 | 夢中的洋流 | T-02-71 梦中的洋流 |
| KnightOfDespair | 100048 | WAW | 魔法少女 | 魔法少女 | O-01-04 憎恶女王 |
| Wraith | 100055 | WAW | 次元衍射变体 | 三維折射變異體 | O-03-88 次元衍射变体 |
| FireBird | 100061 | WAW | 炎雀 | 火之鳥 | O-02-101 炎雀 |
| Yggdrasil | 100062 | WAW | 世界树 | — | （wiki 无页面） |
| Piano | 100065 | WAW | 月光女神 | — | D-01-105 月光女神 |
| Yin | 100104 | WAW | 阴 | 陰 | O-05-102 阴 |
| FengYun | 100105 | WAW | 风云法师 | — | D-01-110 风云法师 |
| MachineDusk | 200003 | WAW | 绿色的黄昏 | 綠色午夜 | 绿色的黄昏 前往何方 |
| CircusDusk | 200007 | WAW | 血色的黄昏 | 녹빛 여명 | 血色的黄昏 绝顶之战 |
| BugDusk | 200015 | WAW | 琥珀色的黄昏 | 綠色黎明 | 琥珀色的黄昏 食物链 |
| MeatIdol | 300004 | WAW | 血肉偶像 | 血肉偶像 | T-09-79 血肉偶像 |
| HellTrain | 300102 | WAW | 黄泉列车 | 地獄特快車 | T-09-86 黄泉列车 |
| ReverseClock | 300110 | WAW | 回溯之钟 | 倒轉之鐘 | D-09-104 回溯之钟 |
| Nullthing | 100005 | ALEPH | 「一无所有」 | 什麼都不是 | O-06-20 「一无所有」 |
| DeathAngel | 100015 | ALEPH | 白夜 | 白夜 | T-03-46 白夜 |
| SilentOrchestra | 100019 | ALEPH | 沉默乐团 | 沉默的管弦樂隊 | T-01-31 沉默乐团 |
| BossBird | 100038 | ALEPH | 终末鸟 | 天啟鳥 | O-02-63 终末鸟 |
| DangoCreature | 100042 | ALEPH | 微笑的尸山 | 微笑的屍山 | T-01-75 微笑的尸山 |
| Censored | 100056 | ALEPH | &lt;color=#FF0000&gt;「CENSORED」&lt;/color&gt; | 刪除 | &lt;color=#FF0000&gt;「CENSORED」&lt;/color&gt; |
| BlueStar | 100058 | ALEPH | 碧蓝新星 | 藍星 | O-03-93 碧蓝新星 |
| SlimeGirl | 100063 | ALEPH | 溶解之爱 | 溶解之愛 | D-03-109 溶解之爱 |
| MachineMidnight | 200004 | ALEPH | 绿色的午夜 | 綠色午夜 | 绿色的午夜 终末螺旋 |
| BugMidnight | 200016 | ALEPH | 琥珀色的午夜 | 綠色午夜 | 琥珀色的午夜 永恒的盛宴 |

## 备注

- 13 个考验小怪（id 200xxx：Bug/Circus/Machine/OutterGod 的 dawn/noon/dusk/midnight 变体）在 cn 文件里部分未汉化（原文如此标注行）；wiki 页面名 =「颜色时段 + 副标题」格式
- `MachineNoon` 游戏文件名英文 "The noon of green"，对应 wiki「绿色的正午 理解的过程」为推断（该文件未汉化）
- `世界树(Yggdrasil)`、`粉红「小分队」(PinkCorps)` 在 wiki 无独立页面
- 再生成：解析上述 XML 的 `<info id>` 与首个 `<name openLevel="\d">` 节点，按 id 排序；wiki 核验用 `zh/api.php?action=query&titles=<名>&redirects=1`（需 User-Agent，默认 python UA 会 403）
