using HarmonyLib;


namespace SupermarketArchipelago
{
    [HarmonyPatch(typeof(StoreLevelManager), nameof(StoreLevelManager.CurrentLevel), MethodType.Getter)]
    public class StoreLevelBypassPatch
    {
        // Global switch to temporarily spoof the player level
        public static bool UseFakeLevel = false;

        [HarmonyPostfix]
        public static void Postfix(ref int __result)
        {
            if (UseFakeLevel) __result = 99;
        }
    }
    [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.Awake))]
    public static class StoreLoadPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            ArchipelagoClient.SetStoreReady(true);
        }
    }

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public static class MenuLoadPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            ArchipelagoClient.SetStoreReady(false);
        }
    }

}
