# Sefira（部门）

路径均相对 `~/workspace/lobotomy-decompiled/Assembly-CSharp/`。

## SefiraEnum（SefiraEnum.cs，顺序已验证）

`MALKUT, YESOD, HOD, NETZACH, TIPERERTH1, TIPERERTH2, GEBURAH, CHESED, BINAH, CHOKHMAH, KETHER, DAAT, DUMMY`

- **坑：Tiphereth 拼作 `TIPERERTH` 且拆成 1/2 两个值**
- 顺序即游戏流程（Malkuth → … → Kether），DAAT 为特殊，DUMMY 为空值

## Sefira（Sefira.cs）——部门运行时模型

- `SefiraEnum sefiraEnum`（只读，私有字段默认 DUMMY）
- `bool agentDeadPenaltyActivated`——员工死亡惩罚已激活；`const float agentDeadPenalty = 50f`（能量上限惩罚值）
- `name / index / indexString / activated / openLevel`
- `allocateMax`（默认 3）——部门可分配员工上限
- `sefiraPassage / passageList / connectedPassageList`（见 map.md）
- `agentList / officerList / creatureList`——部门内单位/收容物集合
- `isolateManagement`（SefiraIsolateManagement）——隔离状态
- 方法：`Activate()`、`GetDepartNodeToArray(int index)`、`GetRandomWayPoint()`、`GetSaveData/LoadData`
- 构造：`Sefira(string name, int index, string indexString, SefiraEnum sefiraEnum)`

## AutoInority 使用处

- `Automaton.AgentOnFixedUpdate`：员工镇压时低血量"移回 Sefira"（TODO 注释处）
- 部门解锁/隔离状态影响工作分配可行性（`IsAvailable` 相关，未深验）
