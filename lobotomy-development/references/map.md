# Map（地图图结构）

路径均相对 `~/workspace/lobotomy-decompiled/Assembly-CSharp/`。

## MapGraph（MapGraph.cs）——单例 `MapGraph.instance`（`loaded` 属性判就绪）

- `MapNode GetNodeById(string id)`
- `void LoadMap()` / `LoadMap(XmlNode nodeRoot, XmlNode edgeRoot)`
- **mod 扩展**：`LoadModMap(Dictionary<string,XmlNode>, XmlNode)` / `LoadModMap_Add` / `LoadModMap_Replace`（BaseMod 加自定义地图）
- `GetGraphNodes() → MapNode[]` / `GetGraphEdges() → MapEdge[]`
- `GetSefiraNodes(string area)` / `GetSefiraPassage(string area)`——按部门名取
- `ActivateArea(string name)` / `RegisterPassage(PassageObjectModel)`

## MapNode（MapNode.cs）

- `string GetId()`——返回地图 XML 的 id 字符串；`GetPosition()`、`GetAreaName()`
- `PassageObjectModel GetAttachedPassage()`——节点所属房间（通道对象）
- `MapEdge[] GetEdges()` / `MapEdge GetEdgeByNode(MapNode)`
- `AddEdge/RemoveEdge(MapEdge)`、`GetTeleportNode(...)`（传送寻路）
- 字段：`isTemporary / closed / connectedCreature / rabbitUnpassable`（兔子镇压路径限制）

## MapEdge（MapEdge.cs）

节点间边（连通性/通行状态，未深入验证）。

## PassageObjectModel（PassageObjectModel.cs）——房间/通道对象（: ObjectModelBase）

- 字段：`centerNode`（MapNode）、`PassageType type`、`passageGroup`、`isActivate`
- `GetId() / GetSefiraName() / GetSefiraEnum() / GetSefira()`
- **`GetEnteredTargets() / GetEnteredTargetsAsArray()`**——区域内单位集合（算"某房间里有谁"的常用入口）
- `GetNodeList() → MapNode[]` / `GetRandomMovableNode()` / `Activate()` / `IsIsolate()`

## AutoInority 使用处

- Graph.xml（嵌 入 mod 程序集，名 `AutoInority.Graph.xml`）：全设施节点图，用于寻路耗时估计
- `CreatureModel.movableNode.GetPassage()`——取异想体所在房间
- 镇压命令（SuppressCommand）走 MapGraph 寻路
