using System.Collections.Generic;

using AutoInority.Creature;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 门检真值表（实例级）：害羞表情 0-4 中 2/3/4 弹回、0/1 放行、animScript 未就绪
    /// 视为弹回且文案记「未知」；另钉 BaseCreatureExt.ViableAtDoor 默认放行基线。
    /// FaceTypeAtDoor 为产品虚缝（GetFaceType 绑 Unity Animator icall，无法离线驱动），
    /// 测试子类注入编程值，ViableAtDoor 本体走真实现。
    /// 文案断言同时覆盖 mojibake 回归（「未知」须为正确 UTF-8）。
    /// </summary>
    [Collection("GameBridge")] // 共享 SkillTypeList 全局单例，跨类串行
    public class ShyLookDoorTest
    {
        private sealed class ScriptedShyLook : ShyLook
        {
            private readonly int? _face;

            internal ScriptedShyLook(CreatureModel creature, int? face) : base(creature)
            {
                _face = face;
            }

            protected override int? FaceTypeAtDoor() => _face;
        }

        private static ShyLook Look(int? face)
        {
            var creature = GameBridge.Bare<CreatureModel>();
            GameBridge.Set(creature, "script", GameBridge.Bare<ShyThing>());
            var metaInfo = GameBridge.Bare<CreatureTypeInfo>();
            // name property 走 dataTable → CreatureDataList 游戏真数据管道；
            // 注入真名条目（未知 key 会触发 Debug.LogError 的引擎 icall，必须绕开）
            var nameList = new CreatureTypeInfo.CreatureDataList { itemName = "name" };
            GameBridge.Set(nameList, "list", new List<CreatureTypeInfo.CreatureData>
            {
                new CreatureTypeInfo.CreatureData { data = "害羞男孩", openLevel = 0 },
            });
            var dataTable = new CreatureTypeInfo.CreatureDataTable();
            dataTable.dictionary["name"] = nameList;
            GameBridge.Set(metaInfo, "dataTable", dataTable);
            GameBridge.Set(creature, "metaInfo", metaInfo);
            return new ScriptedShyLook(creature, face);
        }

        private static bool Check(ShyLook look, out string message)
        {
            var agent = GameBridge.Bare<AgentModel>();
            GameBridge.Set(agent, "name", "小红");
            var skill = new SkillTypeInfo { id = 3, name = "B", calledName = "本能" };
            return look.ViableAtDoor(agent, skill, out message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void CalmFacesPassThrough(int face)
        {
            GameBridge.InjectSkillTypes();
            Assert.True(Check(Look(face), out var message));
            Assert.Null(message);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void StressedFacesBounce(int face)
        {
            GameBridge.InjectSkillTypes();
            Assert.False(Check(Look(face), out var message));
            Assert.NotNull(message);
            // {0}{1}{2} 三个占位按位闭环：员工名/收容物名/表情号
            Assert.Contains("小红", message);
            Assert.Contains("害羞男孩", message);
            Assert.Contains(face.ToString(), message);
            Assert.EndsWith("已弹回", message);
        }

        [Fact]
        public void MissingAnimBouncesWithUnknownText()
        {
            GameBridge.InjectSkillTypes();
            Assert.False(Check(Look(null), out var message));
            Assert.NotNull(message);
            Assert.Contains("未知", message);
            Assert.Contains("小红", message);
            Assert.EndsWith("已弹回", message);
        }

        private sealed class PlainLook : BaseCreatureExt
        {
            internal PlainLook(CreatureModel creature) : base(creature)
            {
            }
        }

        [Fact]
        public void DoorCheckDefaultsToPassThrough()
        {
            // BaseCreatureExt.ViableAtDoor 虚默认：无门口风险的异想体不拦
            GameBridge.InjectSkillTypes();
            var creature = GameBridge.Bare<CreatureModel>();
            GameBridge.Set(creature, "script", GameBridge.Bare<ShyThing>());
            Assert.True(new PlainLook(creature).ViableAtDoor(GameBridge.Bare<AgentModel>(), new SkillTypeInfo(), out var message));
            Assert.Null(message);
        }
    }
}
