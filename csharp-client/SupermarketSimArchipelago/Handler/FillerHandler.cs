using System;
using System.Threading.Tasks;
using UnityEngine;

namespace SupermarketArchipelago
{
    // ==========================================
    // ARCHIPELAGO FILLER HANDLING SYSTEM
    // ==========================================
    public static class FillerHandler
    {
        /// <summary>
        /// Decodes and executes incoming filler items
        /// </summary>
        public static void ProcessFiller(int apItemID)
        {
            switch(apItemID)
            {
                case 501: // Money Boost
                    GiveMoneyBoost();
                    break;
                case 502: // XP Boost
                    GiveXPBoost();
                    break;
                case 503: // Blackfriday
                    ExecuteBlackfriday();
                    break;
                default:
                    Plugin.Log.LogWarning($"Received unknown filler item ID: {apItemID}.");
                    break;
            }
        }

        /// <summary>
        /// Grants a flat money reward directly to the player's account.
        /// </summary>
        private static void GiveMoneyBoost()
        {
            if (MoneyManager.Instance == null)
                return;
            float amount = UnityEngine.Random.Range(50f, 250f);
            // Adds $50 directly to the store funds
            MoneyManager.Instance.MoneyTransition(amount, MoneyManager.TransitionType.CHECKOUT_INCOME);
        }

        /// <summary>
        /// Grants a random fraction of a store level as experience points.
        /// </summary>
        private static void GiveXPBoost()
        {
            if (StoreLevelManager.Instance == null)
                return;

            int xpFraction = UnityEngine.Random.Range(15, 80);

            StoreLevelManager.Instance.AddPoint(xpFraction);
        }


        [HarmonyLib.HarmonyPatch(typeof(CustomerManager), "Update")]
        public static class CustomerManager_Update_Patch
        {
            // Track how many customers are still in the spawn queue
            public static int PendingBlackFridaySpawns = 0;
            private static float _nextSpawnTime = 0f;

            [HarmonyLib.HarmonyPostfix]
            public static void Postfix(CustomerManager __instance)
            {
                // If customers still need to spawn and the spawn interval timer has elapsed
                if (PendingBlackFridaySpawns > 0 && UnityEngine.Time.time >= _nextSpawnTime)
                {
                    try
                    {
                        __instance.SpawnCustomer();
                        PendingBlackFridaySpawns--;

                        
                        _nextSpawnTime = UnityEngine.Time.time + 1.0f;
                    }
                    catch (System.Exception ex)
                    {
                        Plugin.Log.LogError($"[Event] Error spawning periodic customer: {ex.Message}");
                        PendingBlackFridaySpawns = 0; // Cancel on error
                    }
                }
            }
        }

        /// <summary>
        /// Generates a sudden influx of customers in the store
        /// </summary>
        private async static void ExecuteBlackfriday()
        {
            try
            {
                var manager = CustomerManager.Instance;
                if (manager == null)
                    return;

                int crowdSize = UnityEngine.Random.RandomRangeInt(12, 18);
                CustomerManager_Update_Patch.PendingBlackFridaySpawns = crowdSize;

            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"Error at ExecuteBlackfriday: {ex.Message}");
            }
        }
    }
}