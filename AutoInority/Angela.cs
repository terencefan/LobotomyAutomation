using AutoInority.Localization;

using UnityEngine;

namespace AutoInority
{
    internal partial class Angela
    {
        private static LanguageBase _language;

        private static LanguageBase Language
        {
            get
            {
                if (_language == null || _language.Language != GlobalGameManager.instance.Language)
                {
                    _language = GetLanguage(GlobalGameManager.instance.Language);
                }
                return _language;
            }
        }

        public static void Log(string message, bool verbose = false)
        {
            if (verbose && !AutoInority.Log.DebugEnabled)
            {
                return;
            }
            Notice.instance.Send("AddSystemLog", message);
        }

        public static void Announce(string message)
        {
            Say(message);
            Log(message);
        }

        public static void Say(string message)
        {
            AngelaConversationUI.instance.AddAngelaMessage(message);
        }

        private static LanguageBase GetLanguage(SystemLanguage language)
        {
            switch (language)
            {
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:

                    // AutoInority.Log.Info("Simplified chinese resource loaded");
                    return new ChineseSimplified();
                case SystemLanguage.ChineseTraditional:

                    // AutoInority.Log.Info("Traditional chinese resource loaded");
                    return new ChineseTraditional();
                default:

                    // AutoInority.Log.Info("English resource loaded");
                    return new English();
            }
        }

        // 种地管理面板的界面文案（非 Angela 台词，无性格要求）
        public class FarmPanel
        {
            public static string ByDepartment => Language.FarmPanel_ByDepartment;

            public static string ByRiskLevel => Language.FarmPanel_ByRiskLevel;

            public static string ClickHint => Language.FarmPanel_ClickHint;

            public static string Empty => Language.FarmPanel_Empty;

            public static string Escaped => Language.FarmPanel_Escaped;

            public static string Title => Language.FarmPanel_Title;
        }

    }
}