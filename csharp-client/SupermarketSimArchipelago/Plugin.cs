using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System;
using UnityEngine;

namespace SupermarketArchipelago
{

    public static class PluginInfo
    {
        public const string PLUGIN_GUID = "com.lemix028.supermarketsimulator.archipelago";
        public const string PLUGIN_NAME = "Supermarket Simulator Archipelago";
        public const string PLUGIN_VERSION = "0.3.3"; 
    }

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        public static Harmony HarmonyInstance;
        public static SaveManager.ProgressionContainer ProgressionInstance;
        new public static ManualLogSource Log;
        public static BepInEx.Configuration.ConfigFile ConfigFileInstance;
        public override void Load()
        {
            Log = base.Log;
            ConfigFileInstance = Config;

            // Initialize network module
            ArchipelagoClient.Initialize(Log);

            
            MainMenuPatch.ConfigConfigUrl = Config.Bind("Archipelago", "ServerUrl", "localhost:38281", "The IP and Port of your AP server.");
            MainMenuPatch.ConfigSlotName = Config.Bind("Archipelago", "SlotName", "Player", "Your slot name defined in the YAML file.");
            MainMenuPatch.ConfigPassword = Config.Bind("Archipelago", "Password", "", "The password if required by the server.");


            ClassInjector.RegisterTypeInIl2Cpp<ConnectedUi>();
            ClassInjector.RegisterTypeInIl2Cpp<ArchipelagoNotificationManager>();
            ClassInjector.RegisterTypeInIl2Cpp<PowerOutageManager>();
            ClassInjector.RegisterTypeInIl2Cpp<UnityMainThreadDispatcher>();


            GameObject dispatcherObj = new GameObject("UnityMainThreadDispatcher");
            dispatcherObj.AddComponent<UnityMainThreadDispatcher>();
            GameObject.DontDestroyOnLoad(dispatcherObj);


            GameObject apUiObject = new GameObject("Archipelago_UI_Manager");
            apUiObject.AddComponent<ConnectedUi>();
            UnityEngine.Object.DontDestroyOnLoad(apUiObject);

            // Enable hooks
            HarmonyInstance = new Harmony(PluginInfo.PLUGIN_GUID);
            HarmonyInstance.PatchAll();

            ArchipelagoNotificationManager.Create();
            PowerOutageManager.Create();
            //AddComponent<SupermarketArchipelago.SupermarketApDebugTools>();

            Log.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} fully loaded!");
        }


    }





}
