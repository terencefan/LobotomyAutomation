using UnityEngine;

namespace AutoInority.Localization
{
    public abstract class LanguageBase
    {
        public abstract SystemLanguage Language { get; }

        #region Automation

        public abstract string Automation_Conv_Off { get; }

        public abstract string Automation_Conv_On { get; }

        public abstract string Automation_Log_DebugOff { get; }

        public abstract string Automation_Log_DebugOn { get; }

        public abstract string Automation_Log_ExploitOff { get; }

        public abstract string Automation_Log_ExploitOn { get; }

        #endregion Automation

        #region Agent

        public abstract string Agent_Log_GotEGOGift { get; }

        #endregion Agent

        #region ExploitPanel

        public abstract string ExploitPanel_ByDepartment { get; }

        public abstract string ExploitPanel_ByRiskLevel { get; }

        public abstract string ExploitPanel_ClickHint { get; }

        public abstract string ExploitPanel_Empty { get; }

        public abstract string ExploitPanel_Escaped { get; }

        public abstract string ExploitPanel_Title { get; }

        #endregion ExploitPanel
    }
}