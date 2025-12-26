using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using BepInEx.Bootstrap;
using HutongGames.Utility;
using System.Linq;
using System.Reflection;
using System;

namespace SkipKarmelitaArena
{
    [HarmonyPatch]
    [BepInPlugin("io.github.randomscorp.skipkarmelitaarena", "Skip Karmelita Arena", "1.0")]
    public partial class SkipKarmelitaArenaPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> skipKarmelitaArena;

        private Harmony harmony = new("io.github.skipkarmelitaarena");

        private void Awake()
        {
            skipKarmelitaArena = Config.Bind(
                "Gameplay",
                "If Karmelita arena should be skipped",
                true
                );
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(PlayMakerFSM), nameof(PlayMakerFSM.OnEnable))]
        [HarmonyPostfix]
        private static void PatchKarmelitaArena(PlayMakerFSM __instance)
        {
            if (SkipKarmelitaArenaPlugin.skipKarmelitaArena.Value && __instance.FsmName == "Control" && __instance.gameObject.name == "Hunter Queen Boss")
            {
                var state = __instance.FsmStates.First(state => state.name == "Battle Dance");
                state.Actions.First(action => action.GetType() == typeof(SendMessage)).Enabled = false;
                state.Actions = state.Actions.AddToArray<FsmStateAction>(new CustomFSMAction(() =>
                {
                    __instance.SendEvent("BATTLE END");
                }));
            }
        }
        
        public class CustomFSMAction : FsmStateAction
        {
            private Action action;

            public CustomFSMAction(Action action)
            {
                this.action = action;
            }
            public override void OnEnter()
            {
                action();
                base.OnEnter();
            }
        }
    }

}