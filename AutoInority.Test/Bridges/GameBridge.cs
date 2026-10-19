using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace AutoInority.Test
{
    /// <summary>
    /// 游戏 DLL 离线桥接：测试工程挂载真 Assembly-CSharp.dll 后，深构造/引擎耦合的
    /// 游戏类型无法直接 new（构造器级联引擎单例，如 AgentModel → MapGraph.instance），
    /// 按三种桥接方式供给实例：
    /// 1. 跳构造器分配（GetUninitializedObject）+ 反射注字段——纯托管深构造类型
    ///    （AgentModel/CreatureModel/CreatureBase 系/UnitBuf 系），注值后其方法走游戏真实现；
    ///    注意字段初始化器不执行（如 UnitModel._bufList 为 null，须显式注入）。
    /// 2. 惰性单例直注——SkillTypeList.instance.Init / Notice.instance.Observe 均为
    ///    public API，直接调用即完成注入（单例自建，无引擎依赖）。
    /// 3. 产品虚缝——UnityEngine.Object 派生类型托管分配后 == null 恒真（fake null 语义
    ///    走 native 壳比较），且叶子方法绑引擎 icall（如 ShyThingAnim.GetFaceType → Animator），
    ///    测试以子类覆写产品虚方法注入编程值（ShyLook.FaceTypeAtDoor）。
    /// 反射字段名为游戏反编译锚点（私有成员契约），游戏更新若重命名在此显式失败。
    /// 单例约定：SkillTypeList/Notice/Automaton.Instance 为跨用例共享的全局单例，
    /// 桥接用例对其只增不删（游戏无注销 API）；涉单例的测试类同入一个 xunit Collection 串行。
    /// </summary>
    internal static class GameBridge
    {
        /// <summary>跳过构造器分配游戏类型：字段全零，初始化器不执行。</summary>
        internal static T Bare<T>()
        {
            return (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
        }

        /// <summary>
        /// 沿继承链写入字段（含 protected/private），返回 FieldInfo 供后续读取。
        /// 字段按 typeof(T)（声明类型）解析：父类引用注子类独有字段会 MissingFieldException，
        /// 调用处声明类型须覆盖字段落点。
        /// </summary>
        internal static FieldInfo Set<T>(T obj, string field, object value)
        {
            var f = Field(typeof(T), field);
            f.SetValue(obj, value);
            return f;
        }

        internal static object Get<T>(T obj, string field)
        {
            return Field(typeof(T), field).GetValue(obj);
        }

        /// <summary>注入 N/R/W/B/P 五型技能表（id 0-4），供 BaseCreatureExt 静态技能字段真解析。</summary>
        internal static void InjectSkillTypes()
        {
            SkillTypeList.instance.Init(new[]
            {
                new SkillTypeInfo { id = 0, name = "N" },
                new SkillTypeInfo { id = 1, name = "R" },
                new SkillTypeInfo { id = 2, name = "W" },
                new SkillTypeInfo { id = 3, name = "B" },
                new SkillTypeInfo { id = 4, name = "P" },
            });
        }

        /// <summary>
        /// 可过 CanWorkWith 全链的桥接 agent（name 注入 + 链上集合空表）。
        /// 游戏真实现链路：Laetitia 心/精灵祝福/FriendToken 查 _bufList；
        /// HasEquipment 经 _equipment(UnitEquipSpace)——weapon/armor 槽位为 null 时
        /// IL 短路跳过，gifts 遍历 replacedGifts/addedGifts 两列表，故 gifts 注空表、
        /// 槽位留 null 即判「未持有」。CanWorkWith 链新挂检查点时在此补注入。
        /// </summary>
        internal static AgentModel CanWorkAgent(string name = "小红")
        {
            var agent = Bare<AgentModel>();
            Set(agent, "name", name);
            Set(agent, "_bufList", new List<UnitBuf>());
            var gifts = Bare<UnitEGOgiftSpace>();
            Set(gifts, "replacedGifts", new List<EGOgiftModel>());
            Set(gifts, "addedGifts", new List<EGOgiftModel>());
            var equip = Bare<UnitEquipSpace>();
            Set(equip, "gifts", gifts);
            Set(agent, "_equipment", equip);
            return agent;
        }

        /// <summary>
        /// 桥接异想体：bare CreatureModel 注 script（泛型定脚本类型）/metaInfo/
        /// feelingStateCubeBounds/计数器/过载。name 与 riskLevel 走 dataTable 真数据
        /// 管道（未知 key 触发 Debug.LogError icall，先例 LadyLookingAtWallTest）；
        /// cubeBounds 传游戏真数据三元 upperBounds（NormalBound/GoodBound/MaxCube），
        /// 如魔弹射手 Freischutz_stat.txt 的 6/11/18。
        /// </summary>
        internal static CreatureModel Creature<TScript>(string name, string riskLevel, int[] cubeBounds, int counter, bool overloaded = false)
        {
            var creature = Bare<CreatureModel>();
            Set(creature, "script", Bare<TScript>());
            Set(creature, "isOverloaded", overloaded);
            Set(creature, "_qliphothCounter", counter);
            var metaInfo = Bare<CreatureTypeInfo>();
            var dataTable = new CreatureTypeInfo.CreatureDataTable();
            dataTable.dictionary["name"] = DataList("name", name);
            dataTable.dictionary["riskLevel"] = DataList("riskLevel", riskLevel);
            Set(metaInfo, "dataTable", dataTable);
            metaInfo.feelingStateCubeBounds = new FeelingStateCubeBounds { upperBounds = cubeBounds };
            Set(creature, "metaInfo", metaInfo);
            return creature;
        }

        private static CreatureTypeInfo.CreatureDataList DataList(string itemName, string data)
        {
            var list = new CreatureTypeInfo.CreatureDataList { itemName = itemName };
            Set(list, "list", new List<CreatureTypeInfo.CreatureData>
            {
                new CreatureTypeInfo.CreatureData { data = data, openLevel = 0 },
            });
            return list;
        }

        private static FieldInfo Field(Type type, string field)
        {
            for (var t = type; t != null; t = t.BaseType)
            {
                var f = t.GetField(field, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (f != null)
                {
                    return f;
                }
            }
            throw new MissingFieldException(type.Name, field);
        }
    }
}
