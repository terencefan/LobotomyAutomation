using System.Collections.Generic;

using AutoInority.Creature;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 门检真值表（实例级）：害羞平淡及更好放行（平淡无危害加成，按普通计算）、
    /// 低落/崩溃看能力——普通期望置信度严格大于及格线（CreatureEscapeConfidence 缺省
    /// 0.99，与 CheckWorkConfidence 家族同口径）硬上，否则弹回；animScript 未就绪视为
    /// 弹回且文案记「未知」；另钉 BaseCreatureExt.ViableAtDoor 默认放行基线。
    /// FaceTypeAtDoor/ConfidenceAtDoor 为产品虚缝（GetFaceType 绑 Unity Animator icall、
    /// NormalConfidence 计算链级联游戏工作概率管道，均无法离线驱动），测试子类注入编程值，
    /// ViableAtDoor 本体走真实现。
    /// 文案断言同时覆盖 mojibake 回归（「未知」须为正确 UTF-8）与表情中文名，
    /// 另直测 FaceName 全分支（含越界落数字）。
    /// </summary>
    [Collection("GameBridge")] // 共享 SkillTypeList 全局单例，跨类串行
    public class ShyLookDoorTest
    {
        // 及格线用 Automaton.CreatureEscapeConfidence 的缺省 0.99
        private const float Gate = 0.99f;

        private sealed class ScriptedShyLook : ShyLook
        {
            private readonly ShyLook.ShyFace? _face;

            private readonly float _confidence;

            internal ScriptedShyLook(CreatureModel creature, ShyLook.ShyFace? face, float confidence) : base(creature)
            {
                _face = face;
                _confidence = confidence;
            }

            protected override ShyLook.ShyFace? FaceTypeAtDoor() => _face;

            protected override float ConfidenceAtDoor(AgentModel agent, SkillTypeInfo skill) => _confidence;
        }

        private static ShyLook Look(ShyLook.ShyFace? face, float confidence = 0f)
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
            return new ScriptedShyLook(creature, face, confidence);
        }

        private static bool Check(ShyLook look, out string message)
        {
            var agent = GameBridge.Bare<AgentModel>();
            GameBridge.Set(agent, "name", "小红");
            var skill = new SkillTypeInfo { id = 3, name = "B", calledName = "本能" };
            return look.ViableAtDoor(agent, skill, out message);
        }

        [Theory]
        [InlineData((int)ShyLook.ShyFace.Beaming)]
        [InlineData((int)ShyLook.ShyFace.Cheerful)]
        [InlineData((int)ShyLook.ShyFace.Plain)] // 平淡无危害加成，按普通放行
        public void CalmFacesPassThrough(int faceValue)
        {
            // xUnit1000 要求测试类 public，而 ShyFace 是产品 internal 嵌套类型，
            // 故签名以 int 过境、体内还原枚举
            var face = (ShyLook.ShyFace)faceValue;
            GameBridge.InjectSkillTypes();
            Assert.True(Check(Look(face), out var message));
            Assert.Null(message);
        }

        [Theory]
        [InlineData((int)ShyLook.ShyFace.Sullen)]
        [InlineData((int)ShyLook.ShyFace.Distraught)]
        public void StressedFacesBounce(int faceValue)
        {
            var face = (ShyLook.ShyFace)faceValue;
            GameBridge.InjectSkillTypes();
            Assert.False(Check(Look(face), out var message));
            Assert.NotNull(message);
            // {0}{1}{2} 三个占位按位闭环：员工名/收容物名/表情中文名
            Assert.Contains("小红", message);
            Assert.Contains("害羞男孩", message);
            Assert.Contains(ShyLook.FaceName(face), message);
            Assert.EndsWith("已弹回", message);
        }

        [Theory]
        [InlineData((int)ShyLook.ShyFace.Sullen)]
        [InlineData((int)ShyLook.ShyFace.Distraught)]
        public void StressedFacesCapableAgentPushesThrough(int faceValue)
        {
            var face = (ShyLook.ShyFace)faceValue;
            GameBridge.InjectSkillTypes();
            // 坏脸但置信度严格大于及格线：能者硬上，不弹
            Assert.True(Check(Look(face, 0.995f), out var message));
            Assert.Null(message);
        }

        [Fact]
        public void StressedFacesAtGateStillBounce()
        {
            // 及格线语义与 CheckWorkConfidence 家族一致：严格大于才过，等于及格线仍弹
            GameBridge.InjectSkillTypes();
            Assert.False(Check(Look(ShyLook.ShyFace.Sullen, Gate), out var message));
            Assert.NotNull(message);
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

        [Fact]
        public void FaceNameCoversAllMembers()
        {
            Assert.Equal("大笑", ShyLook.FaceName(ShyLook.ShyFace.Beaming));
            Assert.Equal("微笑", ShyLook.FaceName(ShyLook.ShyFace.Cheerful));
            Assert.Equal("平淡", ShyLook.FaceName(ShyLook.ShyFace.Plain));
            Assert.Equal("低落", ShyLook.FaceName(ShyLook.ShyFace.Sullen));
            Assert.Equal("崩溃", ShyLook.FaceName(ShyLook.ShyFace.Distraught));
            // 越界值（游戏版本变更等）落数字显示
            Assert.Equal("9", ShyLook.FaceName((ShyLook.ShyFace)9));
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
