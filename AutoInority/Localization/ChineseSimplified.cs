using UnityEngine;

namespace AutoInority.Localization
{
    internal class ChineseSimplified : English
    {
        public override SystemLanguage Language => SystemLanguage.ChineseSimplified;

        public override string Automation_Conv_On => "交给我吧。你的判断力需要休息，最好是无期限的那种。";

        public override string Automation_Conv_Off => "如你所愿，主管。反正收拾残局的人一直都是你，不差这一次。";

        public override string Automation_Conv_PostureProduction => "日常工作重新交给我吧，毕竟在我的工作记录里，失误那一栏到现在还是空白的。";

        public override string Automation_Conv_PostureSuppress => "能压制威胁的职员上前，其余的撤到安全区域。放心，今天的损失报表不会出现新的名字。";

        public override string Automation_Log_DebugOff => "调试日志已关闭";

        public override string Automation_Log_DebugOn => "调试日志已开启";

        public override string Automation_Log_ExploitOn => "已开始对{0}的自动化工作";

        public override string Automation_Log_ExploitOff => "已结束对{0}的自动化工作";

        public override string Agent_Log_GotEGOGift => "{0}获得了饰品：{1}";

        public override string ExploitPanel_ByDepartment => "按部门";

        public override string ExploitPanel_ByRiskLevel => "按等级";

        public override string ExploitPanel_ClickHint => "点击开关以开发异想体";

        public override string ExploitPanel_Empty => "当前没有轮班中的异想体";

        public override string ExploitPanel_Escaped => "出逃中";

        public override string ExploitPanel_Title => "异想体管理";
    }
}