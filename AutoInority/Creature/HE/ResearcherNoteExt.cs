using System.Linq;
using System.Reflection;

using AutoInority.Extentions;

namespace AutoInority.Creature
{
    internal class ResearcherNoteExt : EquipKitExt
    {
        private readonly FieldInfo _creatureManaged = typeof(ResearcherNote.ResearcherNoteKit).GetField(
            nameof(_creatureManaged),
            BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly FieldInfo _equipElapsedTime = typeof(ResearcherNote.ResearcherNoteKit).GetField(
                    nameof(_equipElapsedTime),
            BindingFlags.NonPublic | BindingFlags.Instance);

        public ResearcherNoteExt(CreatureModel kit) : base(kit)
        {
        }

        // 持有期间物+神累计伤害 >60（_gainedDamage）立即自爆并 30R 溅射
        // （ResearcherNote.cs L200-216）：只交给 HP 与精神均 >=80% 的职员，
        // 把吃满 60 伤害的余量拉到最长
        public override bool CanUse(AgentModel agent)
        {
            return agent.hp >= agent.maxHp * 0.8f && agent.mental >= agent.maxMental * 0.8f;
        }

        public override bool CanReturn(AgentModel agent)
        {
            var elapsed = (float)_equipElapsedTime.GetValue(Script.kitEvent);
            var managed = (bool)_creatureManaged.GetValue(Script.kitEvent);
            return managed && elapsed > 30;
        }

        public override void OnFixedUpdate()
        {
            var agent = _kit.kitEquipOwner;
            var managed = (bool)_creatureManaged.GetValue(Script.kitEvent);

            if (agent != null && agent.IsAvailable() && !managed)
            {
                var creatures = CreatureManager.instance.GetCreatureList().Where(x => x.IsAvailable() && x.IsCreature() && x.GetRiskLevel() < 4);
                var candidates = Candidate.Suggest(agent, creatures);
                if (candidates.Any())
                {
                    candidates.Sort(Comparer);
                    candidates.First().Apply();
                }
            }
            base.OnFixedUpdate();
        }

        private static int Comparer(Candidate x, Candidate y) => x.Distance.CompareTo(y.Distance);
    }
}