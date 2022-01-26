using AutoInority.Localization;

using UnityEngine;

namespace AutoInority
{
    internal class Angela
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

        public static void Log(string message)
        {
            Notice.instance.Send("AddSystemLog", message);
        }

        public static void Say(string message)
        {
            AngelaConversationUI.instance.AddAngelaMessage(message);
        }

        private static LanguageBase GetLanguage(SystemLanguage language)
        {
            switch (language)
            {
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

        public class Agent
        {
            public static string GotEGOGift => Language.Agent_Log_GotEGOGift;

            public static string HasEGOGift => Language.Agent_Conv_HasEGOGift;

            public static string NoEgoGift => Language.Agent_Conv_NoEGOGift;

            public static string ReachMaxExp => Language.Agent_Log_ReachMaxExp;

            public static string SlotLocked => Language.Agent_Conv_SlotLocked;
        }

        public class Automaton
        {
            public static string FarmOff => Language.Automation_Log_FarmOff;

            public static string FarmOn => Language.Automation_Log_FarmOn;

            public static string Off => Language.Automation_Conv_Off;

            public static string On => Language.Automation_Conv_On;
        }

        public class Creature
        {
            public const string BeautyBeast = "根据我们已经获得的资料显示，对虚弱状态下的{1}进行{2}会导致{0}死亡，你是否应该重新审视你的决定？";

            public const string BigBird = "{1}的逆卡巴拉计数器已经很低了，我认为这一工作安排结果的风险很大。";

            public const string BloodBath = "将{0}送入{1}中以提高能量产出确实是一个不错的主意，如果你也这么认为的话。";

            public const string BlueStarDecrease = "";

            public const string BlueStarDie = "";

            public const string Butterfly = "安排{0}去进行{2}大概会激怒{1}吧，不过我想这也不是什么大问题。";

            public const string Censored = "我相信{0}在进入收容单位后一定会陷入恐慌，我很惊讶您没有意识到这个问题。";

            public const string DerFreischutzExt = "{1}对正义有着极高的要求，{0}的工作可能会激怒他导致逆卡巴拉计数器的减少，你是不是没有注意到这一点？";

            public const string Fairy = "您难道没有发现{0}已经拥有{1}的祝福了吗？您这样的主管真是令我失望。";

            public const string FireBird = "{1}和其他异想体不同，我相信你并不想看到工作结果为良或优。";

            public const string FireBirdEdge = "我想告诉你的是，{1}已经在突破的边缘了，这个工作安排并不明智。";

            public const string HappyTeddy = "我很遗憾看到您连续安排{0}对{1}进行{2}，或许下一位主管能做的比您更好些。";

            public const string KnightOfDespair = "我只是提醒一下，{1}还没有祝福过任何员工。";

            public const string Laetitia = "您难道没有发现{0}已经拥有{1}的礼物了吗？您这样的主管真是令我失望。";

            public const string Nothing = "被激怒的{1}很可能会杀死员工或者突破收容，我不认为{0}具备对{1}进行{2}的能力。";

            public const string RedShoes = "我不认为安排{0}去对{1}进行{2}是一个好主意，很明显{0}还不如您自律。";

            public const string Sakura = "本次对{1}的工作将会导致设施内的员工对其着魔，不过我相信你应该已经准备好镇压了。";

            public const string Shark = "安排{0}对{1}进行{2}是非常不明智的行为，你是不是应该多思考一下。";

            public const string SiningMachine = "我相信{0}在结束工作后就会跳进{1}中，这将会是公司的一笔重大财务损失。";

            public const string SnowQueen = "本次对{1}的{2}有可能会导致{0}被冰封，请准备解救工作。";

            public const string SnowQueenDual = "你真的相信{0}能在与{1}的决斗中取胜？至少我不是这么认为的。";

            public const string SnowQueenFire = "我想水火不相容这种浅显的道理，应该不需要我提醒您吧？";

            public const string VoidDream = "我相信{0}在进入{1}的收容单位后会陷入永久的睡眠，你真的有看过观察资料吗？";

            public const string YoungPrinceHasBuf = "我注意到{0}已经连续对{1}进行了三次工作，建议立即安排{0}进行其他异想体的工作以避免彻底的感染。";

            public const string YoungPrinceTwice = "对{1}连续进行洞察以外的工作会强制减少{1}的逆卡巴拉计数器，我很难相信你是故意这么做的。";

            public const string ArmorWarning = "指派{0}对{1}进行{2}会导致其无法再完成其他的沟通和压迫工作，以他目前的能力值来看，我认为他还没有准备好。";

            public const string ArmorKill = "我相信{1}并不喜欢弱小的员工，这可能是我们最后一次见到{0}了。";

            public const string Spider = "我想{0}在完成{1}的{2}后就会被变成茧吧，主管，您真的有在认真读观察记录吗？ ";

            public const string Ppodae = "{1}并不喜欢{2}，恐怕这一工作安排恐怕会导致{1}的出逃，希望你已经准备好了。";

            public const string Porccubus = "以{0}的自律能力应该无法抵挡来自{1}的死亡诱惑吧，你应该开始这位员工的善后准备了。";

            public const string Woodsman = "没有心脏的{1}将会斩首任何接近他的员工，而{0}将会成为因你而死的受害者。";
        }
    }
}