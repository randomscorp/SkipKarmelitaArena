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
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace SkipKarmelitaArena
{
    [HarmonyPatch]
    [BepInPlugin("io.github.randomscorp.skipkarmelitaarena", "Skip Karmelita Arena", "1.1")]
    public partial class SkipKarmelitaArenaPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> skipKarmelitaArena;
        public static ConfigEntry<bool> skipCrawFatherArena;
        public static ConfigEntry<bool> skipUnravelledArena;
        public static ConfigEntry<bool> skipBroodingMotherArena;
        public static ConfigEntry<bool> skipChefLugoliArena;
        public static ConfigEntry<bool> skipGroalArena;
        public static ManualLogSource logger;
        private Harmony harmony = new("io.github.skipkarmelitaarena");

        private void Awake()
        {

            logger = Logger;

            skipKarmelitaArena = Config.Bind(
                "Gameplay",
                "Skip Karmelita Arena?",
                true
                );
            skipCrawFatherArena = Config.Bind(
                "Gameplay",
                "Skip Craw Father Arena?",
                true
                );
            skipUnravelledArena= Config.Bind(
                "Gameplay",
                "Skip Unravelled Arena?",
                true
                );
            skipBroodingMotherArena = Config.Bind(
                "Gameplay",
                "Skip Brooding Mother Arena?",
                true
                );
            skipChefLugoliArena = Config.Bind(
                "Gameplay",
                "Skip Chef Lugoli Arena?",
                true
                );
            skipGroalArena = Config.Bind(
                "Gameplay",
                "Skip Groal The Great Arena?",
                true
                );
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(PlayMakerFSM), nameof(PlayMakerFSM.OnEnable))]
        [HarmonyPrefix]
        private static void PatchFsmArenas(PlayMakerFSM __instance)
        {
            //Patch Karmelita
            if (SkipKarmelitaArenaPlugin.skipKarmelitaArena.Value &&
                __instance.FsmName == "Control" &&
                __instance.gameObject.name == "Hunter Queen Boss")
            {
                var state = __instance.FsmStates.First(state => state.name == "Battle Dance");
                state.Actions.First(action => action.GetType() == typeof(SendMessage)).Enabled = false;
                state.Actions = state.Actions.AddToArray<FsmStateAction>(new CustomFSMAction(() =>
                {
                    __instance.SendEvent("BATTLE END");

                }));
                state.Actions.First(action => action.GetType() == typeof(Wait)).Enabled = false;
            }
            //Patch Unraveled
            else if (SkipKarmelitaArenaPlugin.skipUnravelledArena.Value &&
                __instance.gameObject.scene.name == "Ward_02_boss" &&
                __instance.FsmName == "Control" &&
                __instance.gameObject.name == "Boss Scene")
            {
                var state = __instance.FsmStates.First(state => state.name == "First Slasher");
                state.Actions = new FsmStateAction[]
                {
                new CustomFSMAction(() =>
                {
                    __instance.SetState("P3 Shake");
                    })
                };

                __instance.FsmStates.First(state => state.name == "Boss Title").Actions.First(action => action.GetType() == typeof(Wait)).Enabled = false;
                __instance.FsmStates.First(state => state.name == "Arena Start").Actions.First(action => action.GetType() == typeof(Wait)).Enabled = false;
                __instance.FsmStates.First(state => state.name == "Encountered Pause").Actions.First(action => action.GetType() == typeof(Wait)).Enabled = false;
                __instance.FsmStates.First(state => state.name == "Encountered Start").Actions.First(action => action.GetType() == typeof(Wait)).Enabled = false;
            }
            //skip groal slowmo
            else if (skipGroalArena.Value &&
                __instance.gameObject.scene.name == "Shadow_18" &&
                __instance.FsmName == "Control" &&
                __instance.gameObject.name == "Swamp Shaman")
            {
                __instance.gameObject.LocateMyFSM("Control").FsmStates.First(state => state.name == "Fake Battle End").
                    Actions =  [new CustomFSMAction(() => __instance.SendEvent("FINISHED"))];

            }
            //skip crawfather repeat loop
            else if (SkipKarmelitaArenaPlugin.skipCrawFatherArena.Value && __instance.gameObject.scene.name == "Room_CrowCourt_02")
            {
                if (__instance.FsmName == "Control" &&
                    __instance.gameObject.name == "Crawfather")
                {
                    __instance.FsmStates.First(state => state.name == "Emerge Announce").Actions = [new CustomFSMAction(() =>
                    {
                        __instance.SendEvent("FINISHED");
                    })];
                }
                
                else if (__instance.FsmName == "Battle Start" &&
                    __instance.gameObject.name == "Battle Start")
                {
                    __instance.FsmStates.First(state => state.name == "Crowd Roar").Actions = [new CustomFSMAction(() =>
                    {
                        __instance.SetState("Battle Start");
                    })];

                    __instance.FsmStates.First(state => state.name == "Lights Up").Actions.First(action => action.GetType() == typeof(Wait)).Enabled=false;
                }

            }

        }
        [HarmonyPatch(typeof(BattleScene), nameof(BattleScene.Awake))]
        [HarmonyPrefix]
        private static void PatchBattleSceneArenas(BattleScene __instance)
        {
            if ((__instance.gameObject.scene.name == "Room_CrowCourt_02" && skipCrawFatherArena.Value)||
                (__instance.gameObject.scene.name == "Dust_Chef" && skipChefLugoliArena.Value) ||
                (__instance.gameObject.scene.name == "Shadow_18" && skipGroalArena.Value)||
                (__instance.gameObject.scene.name == "Slab_16b" && skipBroodingMotherArena.Value ))
            {
                __instance.waves = new List<BattleWave> { __instance.waves[__instance.waves.Count()-1] };
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