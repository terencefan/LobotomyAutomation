using System.Collections.Generic;

using AutoInority.Extentions;

namespace AutoInority.Command
{
    internal class RepeatWorkCommand : BaseCommand
    {
        public override bool IsApplicable => Agent.IsAvailable() && Creature.IsAvailable() && !Automaton.InEmergency;

        public override bool IsCompleted
        {
            get
            {
                if (!Automaton.Instance.WorkingAgents.ContainsKey(Agent))
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

        private AgentModel Agent { get; }

        private CreatureModel Creature { get; set; }

        private bool ForExp { get; set; }

        private bool ForGift { get; set; }

        private SkillTypeInfo Skill { get; set; }

        public RepeatWorkCommand(AgentModel agent)
        {
            Agent = agent;
        }

        public override bool Equals(ICommand other) => Equals(other as RepeatWorkCommand);

        public override bool Execute()
        {
            var ext = Creature.GetExtension();
            if (ext.CanWorkWith(Agent, Skill, out _) && ext.CheckConfidence(Agent, Skill))
            {
                Apply();
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hashCode = -1802849804;
            hashCode = hashCode * -1521134295 + EqualityComparer<AgentModel>.Default.GetHashCode(Agent);
            hashCode = hashCode * -1521134295 + EqualityComparer<CreatureModel>.Default.GetHashCode(Creature);
            hashCode = hashCode * -1521134295 + EqualityComparer<RwbpType>.Default.GetHashCode(Skill.rwbpType);
            return hashCode;
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

        public void Update(CreatureModel creature, SkillTypeInfo skill, bool forGift = false, bool forExp = false)
        {
            Creature = creature;
            Skill = skill;
            ForGift = forGift;
            ForExp = forExp;
        }

        private void Apply()
        {
            var sprite = CommandWindow.CommandWindow.CurrentWindow.GetWorkSprite(Skill.rwbpType);
            Agent.ManageCreature(Creature, Skill, sprite);
            Agent.counterAttackEnabled = false;
            Creature.Unit.room.OnWorkAllocated(Agent);
            Creature.script.OnWorkAllocated(Skill, Agent);
            AngelaConversation.instance.MakeMessage(AngelaMessageState.MANAGE_START, Agent, Skill, Creature);
        }

        private bool Equals(RepeatWorkCommand other) => other != null && Equals(Agent, other.Agent);
    }
}