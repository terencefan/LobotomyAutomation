using UnityEngine;

namespace AutoInority.Localization
{
    internal class English : LanguageBase
    {
        public override SystemLanguage Language => SystemLanguage.English;

        public override string Automation_Conv_On => "Automatic ON, sending agents back to their position.";

        public override string Automation_Conv_Off => "Automation OFF.";

        public override string Automation_Log_FarmOn => "Farming with {0} started.";

        public override string Automation_Log_FarmOff => "Farming with {0} ended.";

        public override string Agent_Conv_HasEGOGift => "I think {0} already have the EGO gift: {1}, the task is meaningless.";

        public override string Agent_Conv_SlotLocked => "I think {0} has locked his {1}, the task is meaningless.";

        public override string Agent_Log_GotEGOGift => "{0} got the EGO gift: {1}";

        public override string Agent_Log_ReachMaxExp => "{0} exploited his potential of {1}";
    }
}
