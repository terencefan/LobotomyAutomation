using System.Collections.Generic;
using System.Reflection;

using AutoInority.Creature;

using Xunit;

namespace AutoInority.Test
{
    /// <summary>
    /// 你必须要幸福使用次数护栏（YouMustHappy.cs）：同一员工第 6 次使用即失控死亡
    /// （OnReleaseWork usedCount>=6 → LoseControl+Dead 动画）；buff 四维总和 ≥200
    /// 还会日结猝死。自动化最多用 4 次（第 5 次留给主管手动，第 6 次即死——留一次
    /// 余量）。_usedAgentList 是私有嵌套类 UsedAgent{usedCount,usedAgent} 的列表。
    /// </summary>
    [Collection("GameBridge")]
    public class YouMustBeHappyExtTest
    {
        private static YouMustHappy ScriptWithUsage(AgentModel agent, int usedCount)
        {
            var script = GameBridge.Bare<YouMustHappy>();
            if (agent != null)
            {
                var nested = typeof(YouMustHappy).GetNestedType("UsedAgent", BindingFlags.NonPublic);
                var entry = System.Activator.CreateInstance(nested, agent);
                entry.GetType().GetField("usedCount").SetValue(entry, usedCount);
                var listType = typeof(YouMustHappy)
                    .GetField("_usedAgentList", BindingFlags.NonPublic | BindingFlags.Instance).FieldType;
                var list = System.Activator.CreateInstance(listType);
                listType.GetMethod("Add").Invoke(list, new[] { entry });
                GameBridge.Set(script, "_usedAgentList", list);
            }
            return script;
        }

        private static YouMustBeHappyExt Ext(YouMustHappy script)
        {
            var creature = GameBridge.Bare<CreatureModel>();
            GameBridge.Set(creature, "script", script);
            return new YouMustBeHappyExt(creature);
        }

        [Fact]
        public void CanUseAcceptsFreshAgent()
        {
            // Bare 无构造器：_usedAgentList 字段为 null → 视为 0 次
            Assert.True(Ext(ScriptWithUsage(GameBridge.CanWorkAgent(), 0)).CanUse(GameBridge.CanWorkAgent()));
        }

        [Fact]
        public void CanUseAcceptsThirdUse()
        {
            var agent = GameBridge.CanWorkAgent();
            Assert.True(Ext(ScriptWithUsage(agent, 3)).CanUse(agent));
        }

        [Fact]
        public void CanUseRejectsFourthUse()
        {
            var agent = GameBridge.CanWorkAgent();
            Assert.False(Ext(ScriptWithUsage(agent, 4)).CanUse(agent));
        }

        [Fact]
        public void CanStopUnchanged()
        {
            var script = ScriptWithUsage(null, 0);
            GameBridge.Set(script, "_succeedUsing", true);
            Assert.True(Ext(script).CanStop());
            GameBridge.Set(script, "_succeedUsing", false);
            Assert.False(Ext(script).CanStop());
        }
    }
}
