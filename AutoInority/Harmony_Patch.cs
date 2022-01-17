using System;
using System.Collections.Generic;
using System.Reflection;

using AutoInority.Extentions;

using BinahBoss;

using Harmony;

using UnityEngine;

using Random = System.Random;

namespace AutoInority
{
    public class Harmony_Patch
    {
        public const string ModName = "Lobotomy.terencefan.AutoInority";

        private const BindingFlags FlagsAll = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty;

        private static Dictionary<string, int> _limiter = new Dictionary<string, int>();

        public Harmony_Patch()
        {
            Invoke(() =>
            {
                HarmonyInstance mod = HarmonyInstance.Create(ModName);

                // all these methods have to be public.
                PatchGameManager(mod);
                PatchUnitMouseEventManager(mod);
                PatchCommandWindow(mod);

                PatchAgentModel(mod);
                PatchCreatureModel(mod);

                PatchOrdealManager(mod);
                PatchUseSkill(mod);

                PatchSefira(mod);
                Log.Info("patch success");
            });
        }

        public static void AgentModel_TakeDamage_Postfix(AgentModel __instance, DamageInfo dmg) => Invoke(() => Automaton.Instance.AgentTakeDamage(__instance, dmg));

        public static void CommandWindow_OnClick_Prefix(CommandWindow.CommandWindow __instance, AgentModel actor)
        {
            if (actor == null)
            {
                return;
            }

            switch (__instance.CurrentWindowType)
            {
                case CommandType.Management:
                    if (__instance.CurrentTarget is CreatureModel creature)
                    {
                        var skill = SkillTypeList.instance.GetData(__instance.SelectedWork);
                        Invoke(() => ManagementCreature(actor, creature, skill));
                    }
                    return;
                default:
                    // TODO
                    return;
            }
        }

        public static void CreatureModel_OnFixedUpdate_Postfix(CreatureModel __instance)
        {
            Invoke(() => Automaton.Instance.Creature(__instance), __instance.metaInfo.name, 120, random: true);
        }

        public static void FinishWorkSuccessfully_Postfix(UseSkill __instance)
        {
            Invoke(() => CenterBrain.AddRecord(__instance.agent, __instance.targetCreature, __instance.skillTypeInfo));
            Invoke(() => Automaton.Instance.FinishWork(__instance));
        }

        public static void GameManager_EndGame()
        {
            Invoke(Automaton.Reset);
        }

        public static void OrdealManager_OnFixedUpdated_Postfix(OrdealManager __instance)
        {
            Invoke(() => Automaton.Instance.HandleOrdealManager(__instance), nameof(OrdealManager), 60);
        }

        public static void PatchCastOverload_Prefix()
        {
            Invoke(Automaton.IncreaseOverloadLevel);
        }

        public static void SefiraModel_OnFixedUpdate_Prefix(Sefira __instance)
        {
            Invoke(() => Automaton.Instance.Sefira(__instance), __instance.name, 60, offset: __instance.GetPriority());
        }

        public static void UnitMouseEventManager_Update(UnitMouseEventManager __instance)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                Invoke(Automaton.Instance.Toggle);
            }
            else if (Input.GetKeyDown(KeyCode.O))
            {
                Invoke(Automaton.Instance.ToggleAll);
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                var currentWindow = CommandWindow.CommandWindow.CurrentWindow;
                if (currentWindow.enabled && currentWindow.IsEnabled && currentWindow.CurrentTarget is CreatureModel creature)
                {
                    Log.Info($"{creature.metaInfo.name} enter farm mode");
                }
            }
            else if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                Invoke(() => __instance.GetSelectedAgents().ForEach(x => Automaton.Instance.Remove(x)));
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                var currentWindow = CommandWindow.CommandWindow.CurrentWindow;
                if (currentWindow.enabled && currentWindow.IsEnabled && currentWindow.CurrentTarget is CreatureModel creature)
                {
                    Invoke(() => Automaton.Instance.ToggleFarming(creature));
                }
            }
            else if (Input.GetKeyDown(KeyCode.V))
            {
                Invoke(Automaton.Instance.Clear);
            }
            else if (Input.GetKeyDown(KeyCode.H))
            {
                Invoke(() => SefiraManager.instance.sefiraList.ForEach(x => x.ReturnAgentsToSefira()));
            }
            else if (Input.GetKeyDown(KeyCode.N))
            {
                Invoke(() => SefiraManager.instance.sefiraList.ForEach(x => x.MoveToNeighborPassage()));
            }
            else if (Input.GetKeyDown(KeyCode.B))
            {
                Invoke(() => SefiraManager.instance.sefiraList.ForEach(x => x.MoveToNetzachElevator()));
            }
        }

        public static void UpdateGameSpeed_Postfix()
        {
            var manager = GameManager.currentGameManager;

            if (manager.state == GameState.PLAYING)
            {
                switch (manager.gameSpeedLevel)
                {
                    case 1:
                        Time.timeScale = 1f;
                        Time.fixedDeltaTime = 0.02f;
                        break;

                    case 2:
                        Time.timeScale = 2f;
                        Time.fixedDeltaTime = 0.03f;
                        break;

                    case 3:
                        Time.timeScale = 5f;
                        Time.fixedDeltaTime = 0.05f;
                        break;
                }
            }
            else if (manager.state == GameState.PAUSE)
            {
                Time.timeScale = 0f;
            }
        }

        public void PatchAgentModel(HarmonyInstance mod)
        {
            var postfix = typeof(Harmony_Patch).GetMethod(nameof(AgentModel_TakeDamage_Postfix));
            mod.Patch(typeof(AgentModel).GetMethod(nameof(AgentModel.TakeDamage), new[] { typeof(DamageInfo) }), null, new HarmonyMethod(postfix));
            Log.Info($"patch AgentModel.TakeDamage success");
        }

        public void PatchCommandWindow(HarmonyInstance mod) => PatchPrefix(mod, typeof(CommandWindow.CommandWindow), nameof(CommandWindow.CommandWindow.OnClick), nameof(CommandWindow_OnClick_Prefix));

        public void PatchCreatureModel(HarmonyInstance mod) => PatchPostfix(mod, typeof(CreatureModel), nameof(CreatureModel.OnFixedUpdate), nameof(CreatureModel_OnFixedUpdate_Postfix));

        public void PatchGameManager(HarmonyInstance mod)
        {
            PatchPrefix(mod, typeof(GameManager), nameof(GameManager.EndGame), nameof(GameManager_EndGame));
            PatchPostfix(mod, typeof(GameManager), "UpdateGameSpeed", nameof(UpdateGameSpeed_Postfix));
        }

        public void PatchOrdealManager(HarmonyInstance mod) => PatchPostfix(mod, typeof(OrdealManager), nameof(OrdealManager.OnFixedUpdate), nameof(OrdealManager_OnFixedUpdated_Postfix));

        public void PatchSefira(HarmonyInstance mod) => PatchPrefix(mod, typeof(Sefira), nameof(Sefira.OnFixedUpdate), nameof(SefiraModel_OnFixedUpdate_Prefix));

        public void PatchUnitMouseEventManager(HarmonyInstance mod) => PatchPostfix(mod, typeof(UnitMouseEventManager), "Update", nameof(UnitMouseEventManager_Update));

        public void PatchUseSkill(HarmonyInstance mod) => PatchPostfix(mod, typeof(UseSkill), "FinishWorkSuccessfully", nameof(FinishWorkSuccessfully_Postfix));

        public void PatchWaveOverload(HarmonyInstance mod) => PatchPrefix(mod, typeof(WaveOverload), nameof(WaveOverload.CastOverload), nameof(PatchCastOverload_Prefix));

        private static void Invoke(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                Log.Error(e.StackTrace);
            }
        }

        private static void Invoke(Action action, string k, int interval, int offset = 0, bool random = false)
        {
            var r = new Random();
            if (_limiter.TryGetValue(k, out var last))
            {
                if (Time.frameCount - last < interval)
                {
                    return;
                }
                Invoke(action);
                _limiter[k] = Time.frameCount;
            }
            else
            {
                _limiter[k] = offset - (random ? r.Next(interval) : interval);
            }
        }

        private static void ManagementCreature(AgentModel actor, CreatureModel creature, SkillTypeInfo skill)
        {
            if (!creature.GetCreatureExtension().CanWorkWith(actor, skill, out var message))
            {
                Angela.Say(message);
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                Invoke(() => Automaton.Instance.Register(actor, creature, skill));
            }
            else if (Input.GetKey(KeyCode.LeftControl))
            {
                if (actor.HasEGOGift(creature, out var gift))
                {
                    Angela.Say(string.Format(Angela.HasEGOGift, actor.name, gift.equipTypeInfo.Name));
                    return;
                }
                else if (actor.Equipment.gifts.GetLockState(gift.equipTypeInfo))
                {
                    var slotName = UnitEGOgiftSpace.GetRegionName(UnitEGOgiftSpace.GetRegionId(gift.equipTypeInfo));
                    Angela.Say(string.Format(Angela.SlotLocked, actor.name, slotName));
                    return;
                }
                Automaton.Instance.Register(actor, creature, skill, forGift: true);
            }
            else if (Input.GetKey(KeyCode.LeftAlt))
            {
                Automaton.Instance.Register(actor, creature, skill, forExp: true);
            }
        }

        private void PatchPostfix(HarmonyInstance instance, Type type, string method, string patch, BindingFlags flags = FlagsAll)
        {
            var postfix = typeof(Harmony_Patch).GetMethod(patch);
            instance.Patch(type.GetMethod(method, flags), null, new HarmonyMethod(postfix));
            Log.Info($"patch {type.Name}.{method} success");
        }

        private void PatchPrefix(HarmonyInstance instance, Type type, string method, string patch, BindingFlags flags = FlagsAll)
        {
            var prefix = typeof(Harmony_Patch).GetMethod(patch);
            instance.Patch(type.GetMethod(method, flags), new HarmonyMethod(prefix), null);
            Log.Info($"patch {type.Name}.{method} success");
        }
    }
}