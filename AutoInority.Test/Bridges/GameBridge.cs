using System;
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
