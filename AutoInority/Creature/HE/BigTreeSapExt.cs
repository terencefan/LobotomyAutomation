using System.Reflection;

namespace AutoInority.Creature
{
    internal class BigTreeSapExt : EquipKitExt
    {
        private static readonly FieldInfo ExplodeLevel = typeof(BigTreeSap).GetField("explodeLevel", BindingFlags.NonPublic | BindingFlags.Instance);

        public BigTreeSapExt(CreatureModel kit) : base(kit)
        {
        }

        // 三闸：仅紧急事件中（Automaton.InEmergency：考验在场或存在过载收容物）、
        // 仅 hp<15% 濒死职员、仅每阶段首用——explodeLevel==1 时爆炸概率
        // (1-1)*15%=0%，计数全设施共享且每用 +1（BigTreeSap.cs L38-48），
        // 首用过后本阶段不再启用
        public override bool CanUse(AgentModel agent)
        {
            if (!Automaton.InEmergency || agent.hp >= agent.maxHp * 0.15f)
            {
                return false;
            }
            return Script is BigTreeSap sap && (int)ExplodeLevel.GetValue(sap) == 1;
        }

        // 收益在完工瞬间的治疗 buf，持有无后续价值，用完即还
        public override bool CanReturn(AgentModel agent) => true;
    }
}
