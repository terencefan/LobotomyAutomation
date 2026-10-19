using UnityEngine;

namespace AutoInority.Localization
{
    internal class ChineseSimplified : English
    {
        public override SystemLanguage Language => SystemLanguage.ChineseSimplified;

        public override string Automation_Conv_On => "交给我吧。你的判断力需要休息——最好是无期限的那种。";

        public override string Automation_Conv_Off => "如你所愿，主管。反正收拾残局的人一直都是你，不差这一次。";

        public override string Automation_Log_DebugOff => "调试日志已关闭";

        public override string Automation_Log_DebugOn => "调试日志已开启";

        public override string Automation_Log_FarmOn => "已开始对{0}的自动化工作";

        public override string Automation_Log_FarmOff => "已结束对{0}的自动化工作";

        public override string Agent_Log_GotEGOGift => "{0}获得了饰品：{1}";

        public override string FarmPanel_ByDepartment => "按部门";

        public override string FarmPanel_ByRiskLevel => "按等级";

        public override string FarmPanel_ClickHint => "点击开关以开发异想体";

        public override string FarmPanel_Empty => "当前没有轮班中的异想体";

        public override string FarmPanel_Escaped => "出逃中";

        public override string FarmPanel_Title => "异想体管理";
    }
}