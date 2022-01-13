using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace AutoInority
{
    class Automaton
    {
        private static Automaton _instance;

        private readonly Dictionary<long, List<string>> _caretakers = new Dictionary<long, List<string>>();

        private readonly Dictionary<string, Macro> _macros = new Dictionary<string, Macro>();

        private int OverloadLevel = 1;

        public static Automaton Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Automaton();
                }
                return _instance;
            }
        }

        private bool Running { get; set; } = true;
        public static void IncreaseOverloadLevel()
        {
            Instance.OverloadLevel += 1;
            Log.Info("increase overload level");
        }

        public static void Reset()
        {
            _instance = null;
        }

        /// <summary>
        /// Try finding an agent to ingratiate himself with the creature.
        /// </summary>
        /// <param name="creature"></param>
        public void Apply(CreatureModel creature)
        {
            if (!Running)
            {
                return;
            }

            if (!_caretakers.TryGetValue(creature.metaInfo.id, out var names))
            {
                return;
            }

            foreach (var name in names)
            {
                if (_macros.TryGetValue(name, out var macro) && macro.IsAvailable())
                {
                    macro.Apply();
                    return;
                }
            }
        }

        /// <summary>
        /// Clear all macros.
        /// </summary>
        public void Clear()
        {
            foreach (var macro in _macros.Values)
            {
                Remove(macro.Agent);
            }
        }

        /// <summary>
        /// Complete the macro if the agent got the target EGO gist.
        /// </summary>
        /// <param name="agent"></param>
        public void Invoke(UseSkill skill)
        {
            var agent = skill.agent;

            if (_macros.TryGetValue(agent.name, out var macro))
            {
                if (agent.hp < 0.25 * agent.maxHp || agent.mental < 0.25 * agent.maxMental)
                {
                    Log.Warning($"{agent.name} has to wait until next level");
                    macro.WaitUntilLevel(OverloadLevel);
                }

                if (macro.AimForEGOGift && agent.HasEGOGift(macro.Creature, out var gift))
                {
					Notice.instance.Send("AddSystemLog", $"{skill.agent.Tag()}获得了{gift.equipTypeInfo.Tag()}");
                    Remove(agent);
                }
            }
        }

        public void Register(AgentModel agent, CreatureModel creature, SkillTypeInfo skill, Sprite sprite, bool aimForEGOGift = false)
        {
            try
            {
                _macros[agent.name] = new Macro(aimForEGOGift)
                {
                    Agent = agent,
                    Creature = creature,
                    Skill = skill,
                    Sprite = sprite,
                };
                if (_caretakers.TryGetValue(creature.metaInfo.id, out var agents))
                {
                    agents.Add(agent.name);
                }
                else
                {
                    _caretakers.Add(creature.metaInfo.id, new List<string>() { agent.name });
                }
                Notice.instance.Send("AddSystemLog", $"{agent.Tag()}将会自动对{creature.Tag()}进行{skill.Tag()}");
                agent.AddUnitBuf(new MacroBurf(creature));
            }
            catch (Exception e)
            {
                Log.Warning(e.Message);
                Log.Warning(e.StackTrace);
            }
            finally
            {
                Log.Info("work registered");
            }
        }

        /// <summary>
        /// Remove the macro
        /// </summary>
        /// <param name="agent"></param>
        public void Remove(AgentModel agent)
        {
            if (_macros.TryGetValue(agent.name, out var macro))
            {
                _macros.Remove(agent.name);
                if (_caretakers.TryGetValue(macro.Creature.metaInfo.id, out var names))
                {
                    _caretakers[macro.Creature.metaInfo.id] = new List<string>(names.Where(name => name != agent.name));
                }
            }
            foreach (var buf in agent.GetUnitBufList().Where(buf => buf is MacroBurf).ToList())
            {
                agent.RemoveUnitBuf(buf);
            }
        }

        /// <summary>
        /// Toggle the running status of the Automaton.
        /// </summary>
        public void Toggle()
        {
            Running = !Running;
            var message = Running ? Angela.Automaton.On : Angela.Automaton.Off;
            Angela.Say(message);
        }

        private sealed class Macro
        {
            public readonly bool AimForEGOGift;

            public AgentModel Agent;
            public CreatureModel Creature;

            public SkillTypeInfo Skill;

            public Sprite Sprite;

            private int _levelCounter = 0;

            public Macro(bool forGift = false)
            {
                AimForEGOGift = forGift;
            }

            public void Apply()
            {
                Agent.ManageCreature(Creature, Skill, Sprite);
                Agent.counterAttackEnabled = false;
                Creature.Unit.room.OnWorkAllocated(Agent);
                Creature.script.OnWorkAllocated(Skill, Agent);
                AngelaConversation.instance.MakeMessage(AngelaMessageState.MANAGE_START, Agent, Skill, Creature);
            }

            public bool IsAvailable()
            {
                return Instance.OverloadLevel > _levelCounter && Common.CanWorkWithCreature(Agent, Creature, Skill, silent: true) && Agent.Available() && Creature.Available();
            }

            public void WaitUntilLevel(int level)
            {
                _levelCounter = level;
            }
        }
    }
}
