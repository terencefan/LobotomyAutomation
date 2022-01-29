using System.Collections.Generic;

using AutoInority.Extentions;

namespace AutoInority.Command
{
    internal class RepeatWorkCommand : BaseCommand
    {
        public override bool IsCompleted
        {
            get
            {
                if (!Automaton.Instance.WorkingAgents.Contains(Agent))
                {
                    return true;
                }
                else if (ForGift)
                {
                    if (Agent.HasGift(Creature, out _))
                    {
                        Agent.ResetWaitingPassage();
                        return true;
                    }
                    return false;
                }
                else if (ForExp)
                {
                    if (Agent.HasReachedExpLimit(Skill.rwbpType, out var skillName))
                    {
                        var message = string.Format(Angela.Agent.ReachMaxExp, Agent.Tag(), skillName);
                        Angela.Log(message);
                        return true;
                    }
                    return false;
                }
                return false;
            }
        }

        public override bool IsApplicable => Agent.IsAvailable() && Creature.IsAvailable() && !Automaton.InEmergency;

        private AgentModel Agent { get; }

        private CreatureModel Creature { get; }

        private SkillTypeInfo Skill { get; }

        private bool ForGift { get; }

        private bool ForExp { get; }

        public RepeatWorkCommand(AgentModel agent, CreatureModel creature, SkillTypeInfo skill, bool forGift = false, bool forExp = false)
        {
            Agent = agent;
            Creature = creature;
            Skill = skill;
            ForGift = forGift;
            ForExp = forExp;
        }

        public override bool Execute()
        {
            var ext = Creature.GetExtension();
            if (Agent.IsAvailable() && Creature.IsAvailable() && ext.CanWorkWith(Agent, Skill, out _) && ext.CheckConfidence(Agent, Skill))
            {
                Apply();
                return true;
            }
            return false;
        }

        public override PriorityEnum Priority()
        {
            switch (Creature.GetRiskLevel())
            {
                case 1:
                    return PriorityEnum.MANAGE_CREATURE_1;
                case 2:
                    return PriorityEnum.MANAGE_CREATURE_2;
                case 3:
                    return PriorityEnum.MANAGE_CREATURE_3;
                case 4:
                    return PriorityEnum.MANAGE_CREATURE_4;
                case 5:
                    return PriorityEnum.MANAGE_CREATURE_5;
                default:
                    return PriorityEnum.DEFAULT;
            }
        }

        public override bool Equals(ICommand other) => Equals(other as RepeatWorkCommand);

        public override int GetHashCode()
        {
            int hashCode = -1802849804;
            hashCode = hashCode * -1521134295 + EqualityComparer<AgentModel>.Default.GetHashCode(Agent);
            hashCode = hashCode * -1521134295 + EqualityComparer<CreatureModel>.Default.GetHashCode(Creature);
            hashCode = hashCode * -1521134295 + EqualityComparer<RwbpType>.Default.GetHashCode(Skill.rwbpType);
            return hashCode;
        }

        private bool Equals(RepeatWorkCommand other) => other != null && Equals(Agent, other.Agent) && Equals(Creature, other.Creature) && Equals(Skill.rwbpType, other.Skill.rwbpType);

        private void Apply()
        {
            var sprite = CommandWindow.CommandWindow.CurrentWindow.GetWorkSprite(Skill.rwbpType);
            Agent.ManageCreature(Creature, Skill, sprite);
            Agent.counterAttackEnabled = false;
            Creature.Unit.room.OnWorkAllocated(Agent);
            Creature.script.OnWorkAllocated(Skill, Agent);
            AngelaConversation.instance.MakeMessage(AngelaMessageState.MANAGE_START, Agent, Skill, Creature);
        }
    }
}
