using UnityEngine;

namespace AutoInority.Localization
{
    internal class SimplifiedChinese : English
    {
        public override SystemLanguage Language => SystemLanguage.ChineseSimplified;

        public override string Automation_Conv_On => "自动化工作已开始，员工将会回到他们的岗位上。";

        public override string Automation_Conv_Off => "自动化工作已暂停。";

        public override string Automation_Log_FarmOn => "已开始对{0}的自动化工作";

        public override string Automation_Log_FarmOff => "已结束对{0}的自动化工作";

        public override string Agent_Conv_HasEGOGift => "{0}已经拥有{1}了，这将是个毫无意义的工作安排，我不会帮您自动执行。";

        public override string Agent_Conv_SlotLocked => "{0}的{1}栏已经被锁定了，这是个毫无意义的工作安排，我不会帮您自动执行。";

        public override string Agent_Log_GotEGOGift => "{0}获得了饰品：{1}";

        public override string Agent_Log_ReachMaxExp => "{0}的{1}已经达到最大值";
    }
}
