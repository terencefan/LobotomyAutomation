using System;
using System.Collections.Generic;

using BinahBoss;

using Harmony;

using UnityEngine;

namespace AutoInority
{
    public class Harmony_Patch
    {
        public const string ModName = "Lobotomy.terencefan.AutoInority";

        private static Dictionary<long, DateTime> _limiter = new Dictionary<long, DateTime>();

        public Harmony_Patch()
        {
            try
            {
                HarmonyInstance mod = HarmonyInstance.Create(ModName);
                // all these methods have to be public.
                PatchGameManager(mod);
                PatchUnitMouseEventManager(mod);
                PatchUseSkill(mod);
                PatchCommandWindow(mod);
                PatchCreatureModel(mod);
                PatchWaveOverload(mod);
                Log.Info("patch success");
            }
            catch (Exception ex)
            {
                Log.Warning(ex.Message);
                Log.Warning(ex.StackTrace);
            }
        }

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
                        var spirit = __instance.GetWorkSprite((RwbpType)__instance.SelectedWork);

                        if (actor.CanWorkWith(creature, skill))
                        {
                            if (Input.GetKey(KeyCode.LeftShift))
                            {
                                Automaton.Instance.Register(actor, creature, skill, spirit);
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
                                    Angela.Say(string.Format(Angela.SpaceLocked, actor.name, gift.equipTypeInfo.Name));
                                    return;
                                }
                                Automaton.Instance.Register(actor, creature, skill, spirit, aimForEGOGift: true);
                            }
                        }
                    }
                    return;
                default:
                    // TODO
                    return;
            }
        }

        public static void FinishWorkSuccessfully_Postfix(UseSkill __instance)
        {
            try
            {
                CenterBrain.AddRecord(__instance.agent, __instance.targetCreature, __instance.skillTypeInfo);
                Automaton.Instance.Invoke(__instance);
            }
            catch (Exception e)
            {
                Log.Warning(e.Message);
                Log.Warning(e.StackTrace);
            }
        }

        public static void GameManager_EndGame()
        {
            Automaton.Reset();
            Log.Info("Automaton has been reset");
        }

        public static void OnFixedUpdate_Postfix(CreatureModel __instance)
        {
            var id = __instance.metaInfo.id;
            if (_limiter.TryGetValue(id, out var last))
            {
                if (DateTime.UtcNow - last < TimeSpan.FromSeconds(1))
                {
                    return;
                }
            }
            Automaton.Instance.Apply(__instance);
            _limiter[id] = DateTime.UtcNow;
        }

        public static void PatchBinahOverload_CastOverload()
        {
            Automaton.IncreaseOverloadLevel();
        }

        public static void UnitMouseEventManager_Update(UnitMouseEventManager __instance)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                Automaton.Instance.Toggle();
            }
            else if (Input.GetKeyDown(KeyCode.LeftShift) && __instance.GetSelectedAgents().Count > 0)
            {
                foreach (var agent in __instance.GetSelectedAgents())
                {
                    Automaton.Instance.Remove(agent);
                }
            }
            else if (Input.GetKeyDown(KeyCode.V))
            {
                Automaton.Instance.Clear();
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
        public void PatchCommandWindow(HarmonyInstance mod)
        {
            var prefix = typeof(Harmony_Patch).GetMethod(nameof(CommandWindow_OnClick_Prefix));
            mod.Patch(typeof(CommandWindow.CommandWindow).GetMethod("OnClick"), new HarmonyMethod(prefix), null, null);
            Log.Info(nameof(PatchCommandWindow) + " succcess");
        }

        public void PatchCreatureModel(HarmonyInstance mod)
        {
            var postfix = typeof(Harmony_Patch).GetMethod(nameof(OnFixedUpdate_Postfix));
            mod.Patch(typeof(CreatureModel).GetMethod("OnFixedUpdate"),
                      null,
                      new HarmonyMethod(postfix),
                      null);
            Log.Info(nameof(CreatureModel) + " succcess");
        }

        public void PatchGameManager(HarmonyInstance mod)
        {
            var type = typeof(GameManager);

            // patch gamespeed
            var method1 = typeof(Harmony_Patch).GetMethod(nameof(UpdateGameSpeed_Postfix));
            mod.Patch(type.GetMethod("UpdateGameSpeed", AccessTools.all),
                      null,
                      new HarmonyMethod(method1));

            // patch endgame
            var method2 = typeof(Harmony_Patch).GetMethod(nameof(GameManager_EndGame));
            mod.Patch(type.GetMethod("EndGame"),
                      new HarmonyMethod(method2),
                      null);

            Log.Info(nameof(PatchGameManager) + " succcess");
        }

        public void PatchUnitMouseEventManager(HarmonyInstance mod)
        {
            var method = typeof(Harmony_Patch).GetMethod(nameof(UnitMouseEventManager_Update));
            mod.Patch(typeof(UnitMouseEventManager).GetMethod("Update", AccessTools.all),
                      null,
                      new HarmonyMethod(method),
                      null);
            Log.Info(nameof(PatchUnitMouseEventManager) + " succcess");
        }

        public void PatchUseSkill(HarmonyInstance mod)
        {
            var postfix = typeof(Harmony_Patch).GetMethod(nameof(FinishWorkSuccessfully_Postfix));
            mod.Patch(typeof(UseSkill).GetMethod("FinishWorkSuccessfully", AccessTools.all), null, new HarmonyMethod(postfix));
            Log.Info(nameof(PatchUseSkill) + " succcess");
        }

        public void PatchWaveOverload(HarmonyInstance mod)
        {
            var method = typeof(Automaton).GetMethod(nameof(PatchBinahOverload_CastOverload));
            mod.Patch(typeof(WaveOverload).GetMethod("CastOverload", AccessTools.all), new HarmonyMethod(method), null);
            Log.Info(nameof(PatchWaveOverload) + " succcess");
        }
    }
}