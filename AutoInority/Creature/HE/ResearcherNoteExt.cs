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

        public override bool CanReturn(AgentModel agent)
        {
            var elapsed = (float)_equipElapsedTime.GetValue(_script.kitEvent);
            var managed = (bool)_creatureManaged.GetValue(_script.kitEvent);
            return managed && elapsed > 30;
        }

        public override void OnFixedUpdate()
        {
            var agent = _kit.kitEquipOwner;
            var managed = (bool)_creatureManaged.GetValue(_script.kitEvent);

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