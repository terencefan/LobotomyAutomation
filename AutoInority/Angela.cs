namespace AutoInority
{
    class Angela
    {
        public const string HasEGOGift = "我认为{0}已经拥有{1}了，这是个毫无意义的工作安排，我不会帮你执行。";

        public const string SpaceLocked = "{0}的饰品栏已经被锁定}了，这是个毫无意义的工作安排，我不会帮你执行。";

        public static void Say(string message)
        {
            AngelaConversationUI.instance.AddAngelaMessage(message);
        }

        public class Automaton
        {
            public const string Off = "我已经暂停了所有的自动化工作，祝您工作顺利。";

            public const string On = "自动化工作已经开始，员工将会回到他们自己的岗位上。";
        }

        public class Creatures
        {
            public const string HappyTeddy = "我很遗憾看到您连续安排{0}对{1}进行{2}，或许下一位主管能做的比你更好些。";

            public const string Laetitia = "你难道没有发现{0}已经拥有{1}的礼物了吗？你这样的主管真是令我失望。";

            public const string RedShoes = "我不认为安排{0}去对{1}进行工作是一个好主意，很明显{0}还不如您自律。";

            public const string SiningMachine = "我相信{0}在结束工作后就会跳进{1}中，这将会是公司的一笔重大财务损失。";

            public const string Nothing = "被激怒的{1}很可能会杀死员工或者突破收容，我不认为{0}具备对{1}进行{2}工作的能力。";
        }
    }
}
