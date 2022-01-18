namespace AutoInority
{
    internal class Angela
    {
        public const string HasEGOGift = "我认为{0}已经拥有{1}了，这是个毫无意义的工作安排，我不会帮您执行。";

        public const string SlotLocked = "{0}的{1}栏已经被锁定了，这是个毫无意义的工作安排，我不会帮您执行。";

        public static void Say(string message)
        {
            AngelaConversationUI.instance.AddAngelaMessage(message);
        }

        public class Automaton
        {
            public const string Off = "我已经暂停了所有的自动化工作，祝您工作顺利。";

            public const string On = "自动化工作已经开始，员工将会回到他们自己的岗位上。";

            public const string All = "自动化工作将会以最大功率进行，希望不会惹出什么大麻烦。";
        }

        public class Creatures
        {
            public const string Bad = "{0}对{1}进行{2}的结果有可能是差，这将会导致逆卡巴拉计数器的减少，您确定要这样安排吗？";

            public const string Normal = "{0}对{1}进行{2}的结果有可能是良，这将会导致逆卡巴拉计数器的减少，您确定要这样安排吗？";

            public const string Good = "{0}对{1}进行{2}的结果有可能是优，这将会导致逆卡巴拉计数器的减少，您确定要这样安排吗？";

            public const string HappyTeddy = "我很遗憾看到您连续安排{0}对{1}进行{2}，或许下一位主管能做的比您更好些。";

            public const string Laetitia = "您难道没有发现{0}已经拥有{1}的礼物了吗？您这样的主管真是令我失望。";

            public const string Fairy = "您难道没有发现{0}已经拥有{1}的祝福了吗？您这样的主管真是令我失望。";

            public const string RedShoes = "我不认为安排{0}去对{1}进行{2}是一个好主意，很明显{0}还不如您自律。";

            public const string SiningMachine = "我相信{0}在结束工作后就会跳进{1}中，这将会是公司的一笔重大财务损失。";

            public const string Nothing = "被激怒的{1}很可能会杀死员工或者突破收容，我不认为{0}具备对{1}进行{2}的能力。";

            public const string Censored = "我相信{0}在进入收容单位后一定会陷入恐慌，我很惊讶您没有意识到这个问题。";

            public const string BeautyBeast = "根据我们已经获得的资料显示，对虚弱状态下的{1}进行{2}会导致{0}死亡，你是否应该重新审视你的决定？";

            public const string YoungPrinceHasBuf = "我注意到{0}已经连续对{1}进行了三次工作，建议立即安排{0}进行其他异想体的工作以避免彻底的感染。";

            public const string YoungPrinceTwice = "对{1}连续进行洞察以外的工作会强制减少{1}的逆卡巴拉计数器，我很难相信你是故意这么做的。";

            public const string FireBird = "{1}和其他异想体不同，我相信你并不想看到工作结果为良或优。";

            public const string FireBirdEdge = "我想告诉你的是，{1}已经在突破的边缘了，这个工作安排并不明智。";

            public const string BigBird = "{1}的逆卡巴拉计数器已经很低了，我认为这一工作安排结果的风险很大。";

            public const string Shark = "安排{0}对{1}进行{2}是非常不明智的行为，你是不是应该多思考一下。";
            
            public const string DerFreischutzExt = "{1}对正义有着极高的要求，{0}的工作可能会激怒他导致逆卡巴拉计数器的减少，你是不是没有注意到这一点？";

            public const string SnowQueen = "本次对{1}的{2}有可能会导致{0}被冰封，请准备解救工作";

            public const string SnowQueenDual = "本次对{1}的{2}有可能会导致{0}被冰封，请准备解救工作";
        }
    }
}