using System.Collections.Generic;

using AutoInority.Creature;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 门检真值表（实例级）：害羞平淡及更好放行（平淡无危害加成，按普通计算）、
    /// 低落/崩溃看能力——冻结脸修正后的真实置信度严格大于及格线（CreatureEscapeConfidence
    /// 缺省 0.99，与 CheckWorkConfidence 家族同口径）硬上，否则弹回；animScript 未就绪
    /// 视为弹回且文案记「未知」；另钉 BaseCreatureExt.DoorCheck 默认放行基线。
    /// FaceTypeAtDoor/ProbAtDoor 为产品虚缝（GetFaceType 绑 Unity Animator icall、成功率
    /// 链级联游戏工作概率管道，均无法离线驱动），测试子类注入编程值；ConfidenceAtDoor
    /// 本体走真实现（ prob → Confidence.InRange 重卷积，钉住「修正发生在概率层」语义，
    /// bounds 注入真实害羞数值 NormalBound 6 / MaxCube 12）。
    /// 文案断言同时覆盖 mojibake 回归（「未知」须为正确 UTF-8）与表情中文名，
    /// 另直测 FaceName 全分支（含越界落数字）。
    /// </summary>
    [Collection("GameBridge")] // 共享 SkillTypeList 全局单例，跨类串行
    public class ShyLookDoorTest
    {
        private sealed class ScriptedShyLook : ShyLook
        {
            private readonly ShyLook.ShyFace? _face;

            private readonly float _prob;

            internal ScriptedShyLook(CreatureModel creature, ShyLook.ShyFace? face, float prob) : base(creature)
            {
                _face = face;
                _prob = prob;
            }

            protected override ShyLook.ShyFace? FaceTypeAtDoor() => _face;

            protected override float ProbAtDoor(AgentModel agent, SkillTypeInfo skill) => _prob;

            internal float ExposeDamageMultiplier(AgentModel agent, SkillTypeInfo skill) => GetDamageMultiplierInWork(agent, skill);
        }

        private static ShyLook Look(ShyLook.ShyFace? face, float prob = 0.2f)
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
            // 真实害羞数值：NormalBound 6（普通线 X≥7）、MaxCube 12——#92 闸门数学同一组参数
            metaInfo.feelingStateCubeBounds = new FeelingStateCubeBounds { upperBounds = new[] { 6, 9, 12 } };
            GameBridge.Set(creature, "metaInfo", metaInfo);
            return new ScriptedShyLook(creature, face, prob);
        }

        private static bool Check(ShyLook look, out string message)
        {
            var agent = GameBridge.Bare<AgentModel>();
            GameBridge.Set(agent, "name", "小红");
            // 门检总入口先查心之归属地（HasUnitBuf 走 _bufList），空表即无 buf
            GameBridge.Set(agent, "_bufList", new List<UnitBuf>());
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
            // 缺省 prob 0.2：好脸不查置信度，一律放行
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
            // 缺省 prob 0.2 → 普通期望置信度 ≈0.4%，远不过线：坏脸弹回
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
            // 毕业员工 p=0.9 → 普通期望置信度 ≈0.999 严格大于及格线：能者硬上，不弹
            Assert.True(Check(Look(face, 0.9f), out var message));
            Assert.Null(message);
        }

        [Theory]
        [InlineData((int)ShyLook.ShyFace.Sullen, 0.8f, false)] // p=0.8 → 置信度 ≈0.980，不过线
        [InlineData((int)ShyLook.ShyFace.Distraught, 0.8f, false)]
        [InlineData((int)ShyLook.ShyFace.Sullen, 0.9f, true)]  // p=0.9 → 置信度 ≈0.999，过线
        [InlineData((int)ShyLook.ShyFace.Distraught, 0.9f, true)]
        public void GateBoundaryFollowsBinomialMath(int faceValue, float prob, bool shouldPass)
        {
            // #92 闸门数学：P(X≥7|B(12,p))>0.99 ⟺ p≳0.826，0.8/0.9 夹住阈值——
            // 钉住「门口把脸修正在概率层算回后重卷积」而非在置信度上线性加减
            var face = (ShyLook.ShyFace)faceValue;
            GameBridge.InjectSkillTypes();
            Assert.Equal(shouldPass, Check(Look(face, prob), out _));
        }

        [Fact]
        public void DispatchDamageStaysAtPlainBaseline()
        {
            // 派工口径：CheckSurvive 的伤害倍率恒为 Plain 基线 1.0x（实际伤害由游戏按冻结脸结算）
            GameBridge.InjectSkillTypes();
            var look = (ScriptedShyLook)Look(ShyLook.ShyFace.Plain);
            Assert.Equal(1f, look.ExposeDamageMultiplier(GameBridge.Bare<AgentModel>(), new SkillTypeInfo()));
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
            // BaseCreatureExt.DoorCheck 虚默认：无门口风险的异想体不拦
            // （ViableAtDoor 总入口先过心之归属地，agent 注空 buf 表放行）
            GameBridge.InjectSkillTypes();
            var creature = GameBridge.Bare<CreatureModel>();
            GameBridge.Set(creature, "script", GameBridge.Bare<ShyThing>());
            var agent = GameBridge.Bare<AgentModel>();
            GameBridge.Set(agent, "_bufList", new List<UnitBuf>());
            Assert.True(new PlainLook(creature).ViableAtDoor(agent, new SkillTypeInfo(), out var message));
            Assert.Null(message);
        }
    }
}
