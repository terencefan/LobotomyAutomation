using UnityEngine;

namespace AutoInority.Localization
{
    public abstract class LanguageBase
    {
        public abstract SystemLanguage Language { get; }

        #region Automation

        public abstract string Automation_Conv_Off { get; }

        public abstract string Automation_Conv_On { get; }

        public abstract string Automation_Log_FarmOff { get; }

        public abstract string Automation_Log_FarmOn { get; }

        #endregion Automation

        #region Agent

        public abstract string Agent_Conv_HasEGOGift { get; }

        public abstract string Agent_Conv_NoEGOGift { get; }

        public abstract string Agent_Conv_SlotLocked { get; }

        public abstract string Agent_Log_GotEGOGift { get; }

        public abstract string Agent_Log_ReachMaxExp { get; }

        #endregion Agent
    }
}