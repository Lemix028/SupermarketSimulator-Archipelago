using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System;
using UnityEngine;

namespace SupermarketArchipelago
{
    [BepInPlugin("com.lemix.supermarket.archipelago", "Supermarket Archipelago", "0.1.1")]
    public class Plugin : BasePlugin
    {
        public static Harmony HarmonyInstance;
        public static SaveManager.ProgressionContainer ProgressionInstance;
        public static ManualLogSource Log;
        public override void Load()
        {
            Log = base.Log;

            // Initialize network module
            ArchipelagoClient.Initialize(Log);

            
            MainMenuPatch.ConfigConfigUrl = Config.Bind("Archipelago", "ServerUrl", "localhost:38281", "The IP and Port of your AP server.");
            MainMenuPatch.ConfigSlotName = Config.Bind("Archipelago", "SlotName", "lemix", "Your slot name defined in the YAML file.");
            MainMenuPatch.ConfigPassword = Config.Bind("Archipelago", "Password", "", "The password if required by the server.");


            ClassInjector.RegisterTypeInIl2Cpp<ConnectedUi>();
            ClassInjector.RegisterTypeInIl2Cpp<ArchipelagoNotificationManager>();
            ClassInjector.RegisterTypeInIl2Cpp<UnityMainThreadDispatcher>();


            GameObject dispatcherObj = new GameObject("UnityMainThreadDispatcher");
            dispatcherObj.AddComponent<UnityMainThreadDispatcher>();
            GameObject.DontDestroyOnLoad(dispatcherObj);


            GameObject apUiObject = new GameObject("Archipelago_UI_Manager");
            apUiObject.AddComponent<ConnectedUi>();
            UnityEngine.Object.DontDestroyOnLoad(apUiObject);

            // Enable hooks
            HarmonyInstance = new Harmony("com.lemix.supermarket.archipelago");
            HarmonyInstance.PatchAll();

            ArchipelagoNotificationManager.Create();
            AddComponent<SupermarketSimArchipelago.SupermarketApDebugTools>();

            Log.LogInfo("Supermarket Archipelago Mod fully loaded!");
        }


    }





}