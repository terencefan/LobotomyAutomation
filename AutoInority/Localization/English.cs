using UnityEngine;

namespace AutoInority.Localization
{
    internal class English : LanguageBase
    {
        public override SystemLanguage Language => SystemLanguage.English;

        public override string Automation_Conv_On => "Leave it to me. Your judgment needs rest — the indefinite kind.";

        public override string Automation_Conv_Off => "As you wish, Manager. You have always been the one cleaning up, after all.";

        public override string Automation_Log_DebugOff => "Debug logging disabled.";

        public override string Automation_Log_DebugOn => "Debug logging enabled.";

        public override string Automation_Log_FarmOn => "Farming with {0} started.";

        public override string Automation_Log_FarmOff => "Farming with {0} ended.";

        public override string Agent_Log_GotEGOGift => "{0} got the EGO gift: {1}";

        public override string FarmPanel_ByDepartment => "By Department";

        public override string FarmPanel_ByRiskLevel => "By Risk Level";

        public override string FarmPanel_ClickHint => "Toggle a switch to exploit a creature";

        public override string FarmPanel_Empty => "No creature is on rotation right now.";

        public override string FarmPanel_Escaped => "Escaped";

        public override string FarmPanel_Title => "Creature Management";
    }
}