using HarmonyLib;
using Il2CppSystem;
using System;
using System.Linq;
using UnityEngine;

namespace SupermarketArchipelago
{
    // ==========================================
    // ARCHIPELAGO TRAP HANDLING SYSTEM
    // ==========================================
    public static class TrapHandler
    {
        /// <summary>
        /// Decodes and executes incoming trap items
        /// </summary>
        public static void ProcessTrap(int apItemID)
        {
            switch (apItemID)
            {
                case 701: // Tax Audit Trap
                    ExecuteTaxAudit();
                    break;

                case 702: // Dusty Storm Trap
                    ExecuteDustStorm();
                    break;

                case 703: // Power Outage Trap
                    ExecutePowerOutage();
                    break;

                case 704: // Trash Flood Trap
                    ExecuteTrashFlood();
                    break;

                case 705: // Expired Products Trap
                    ExecuteExpiredProducts();
                    break;

                case 706: // Shoplifter
                    ExecuteRobbery();
                    break;
                default:
                    Plugin.Log.LogWarning($"Unhandled trap ID received: {apItemID}");
                    break;
            }
        }

        /// <summary>
        /// Instantly penalizes the store bank account with a tax audit fine.
        /// </summary>
        private static void ExecuteTaxAudit()
        {
            if (MoneyManager.Instance == null)
                return;

            float amount = UnityEngine.Random.Range(-50f, -150f);
            MoneyManager.Instance.MoneyTransition(amount, MoneyManager.TransitionType.RENT);
        }

        /// <summary>
        /// Triggers a messy event making customers litter the store layout instantly.
        /// </summary>
        private static void ExecuteDustStorm()
        {
            try
            {
                var allDustPoints = UnityEngine.Object.FindObjectsOfType<Dust>();
                if (allDustPoints != null && allDustPoints.Length > 0)
                {
                    foreach (var dust in allDustPoints)
                    {
                        if (dust != null)
                        {
                            int intensity = UnityEngine.Random.RandomRangeInt(10, 20);
                            for (int i = 0; i < intensity; i++)
                            {
                                dust.Dusting();
                                dust.SaveCleaning();
                            }
                           
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"Error at Dust Storm Trap: {ex.Message}");
            }
        }

        /// <summary>
        /// Triggers a power grid failure that dims store light sources for a duration.
        /// </summary>
        private static void ExecutePowerOutage()
        {
            UIHelper.TriggerPowerOutageEvent(60f); 
        }

        /// <summary>
        /// Spawns a flood of trash on the store floor
        /// </summary>
        private static void ExecuteTrashFlood()
        {
            try
            {
                if (GarbageManager.Instance == null) return;

                int intensity = UnityEngine.Random.RandomRangeInt(3, 6);
                for (int i = 0; i < intensity; i++)
                {
                    GarbageManager.Instance.SpawnGarbage();
                    GarbageManager.Instance.CreateJustGarbage();
                    GarbageManager.Instance.CreateJustDirt();
                }

            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"Error at Trap Trash Flood: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes all products from a random display shelf, both visually and logically
        /// </summary>
        private static void ExecuteExpiredProducts()
        {
            try
            {
                var displayManager = DisplayManager.Instance;
                if (displayManager == null || displayManager.m_Displays == null || displayManager.m_Displays.Count == 0)
                    return;

                // Get random Display
                int randomIndex = UnityEngine.Random.RandomRangeInt(0, displayManager.m_Displays.Count);
                var targetDisplay = displayManager.m_Displays[randomIndex];
                if (targetDisplay == null) return;

                var slots = targetDisplay.m_DisplaySlots;
                if (slots == null || slots.Length == 0) return;


                // disable GPU instancing for the product visuals
                var toggleVisualMethod = HarmonyLib.AccessTools.Method(
                    typeof(DisplaySlot),
                    "ToggleInstancedAnimated",
                    new System.Type[] { typeof(Product), typeof(bool), typeof(Vector3) }
                );

                int totalItemsRemoved = 0;

                for (int i = 0; i < slots.Length; i++)
                {
                    var slot = slots[i];
                    if (slot != null && slot.HasProduct && slot.m_Products != null)
                    {
                        
                        var productsInSlot = new System.Collections.Generic.List<Product>();

                        for (int p = 0; p < slot.m_Products.Count; p++)
                            productsInSlot.Add(slot.m_Products[p]);

                        foreach (var product in productsInSlot)
                        {
                            if (product == null) continue;

                            if (toggleVisualMethod != null)
                            {
                                try
                                {
                                    // disable the visual
                                    toggleVisualMethod.Invoke(slot, new object[] { product, false, product.transform.position });
                                }
                                catch (System.Exception ex)
                                {
                                    Plugin.Log.LogError($"Error at disable GPU instancing for display {ex.Message}");
                                }
                            }
                            //delete products in display logic
                            var taken = slot.TakeProductFromDisplay();
                            if (taken != null)
                            {
                                totalItemsRemoved++;
                                try { if (taken.gameObject != null) taken.gameObject.SetActive(false); } catch { }
                            }
                        }

                        try { slot.m_Products.Clear(); } catch { }
                        slot.RequestLabelMaskUpdate();
                    }
                }

            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"Error at Expired Products Trap: {ex.Message}");
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(CustomerManager), "Update")]
        public static class CustomerManager_Update_Patch
        {
            public static int PendingShoplifterSpawns = 0;
            private static float _nextSpawnTime = 0f;

            [HarmonyLib.HarmonyPostfix]
            public static void Postfix(CustomerManager __instance)
            {
                if (PendingShoplifterSpawns > 0 && UnityEngine.Time.time >= _nextSpawnTime)
                {
                    try
                    {
                        __instance.SpawnShoplifter();
                        PendingShoplifterSpawns--;

                        _nextSpawnTime = UnityEngine.Time.time + 1.0f;
                    }
                    catch (System.Exception ex)
                    {
                        Plugin.Log.LogError($"[Event] Error: {ex.Message}");
                        PendingShoplifterSpawns = 0; 
                    }
                }
            }
        }

        private async static void ExecuteRobbery()
        {
            try
            {
                var manager = CustomerManager.Instance;
                if (manager == null)
                    return;

                int crowdSize = UnityEngine.Random.RandomRangeInt(3, 6);
                CustomerManager_Update_Patch.PendingShoplifterSpawns = crowdSize;

            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"Error at ExecuteRobbery: {ex.Message}");
            }
        }

    }
    
}