using UnityEngine;

namespace AutoInority.Localization
{
    internal class English : LanguageBase
    {
        public override SystemLanguage Language => SystemLanguage.English;

        public override string Automation_Conv_On => "Leave it to me. Your judgment needs rest, the indefinite kind.";

        public override string Automation_Conv_Off => "As you wish, Manager. You have always been the one cleaning up, after all.";

        public override string Automation_Conv_PostureProduction => "Day-to-day assignments are mine again, Manager. Even now, the mistake column in my work record remains empty.";

        public override string Automation_Conv_PostureSuppress => "The capable move in, the rest fall back to safety. Do not worry, today's loss report will not carry any new names.";

        public override string Automation_Log_DebugOff => "Debug logging disabled.";

        public override string Automation_Log_DebugOn => "Debug logging enabled.";

        public override string Automation_Log_ExploitOn => "Exploitation of {0} started.";

        public override string Automation_Log_ExploitOff => "Exploitation of {0} ended.";

        public override string Agent_Log_GotEGOGift => "{0} got the EGO gift: {1}";

        public override string ExploitPanel_ByDepartment => "By Department";

        public override string ExploitPanel_ByRiskLevel => "By Risk Level";

        public override string ExploitPanel_ClickHint => "Toggle a switch to exploit a creature";

        public override string ExploitPanel_Empty => "No creature is on rotation right now.";

        public override string ExploitPanel_Escaped => "Escaped";

        public override string ExploitPanel_Title => "Creature Management";
    }
}