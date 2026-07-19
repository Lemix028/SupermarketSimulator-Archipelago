using System;
using HarmonyLib;
using UnityEngine;
using TMPro;

namespace SupermarketArchipelago
{
    [HarmonyPatch(typeof(CartManager), nameof(CartManager.Start))]
    public static class CustomizationAssetsPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (ArchipelagoConfig.FreeCustomizables)
            {
                try
                {
                    var buckets = Resources.FindObjectsOfTypeAll<__Project__.Scripts.WallPaintSystem.BucketSo>();
                    if (buckets != null && buckets.Length > 0)
                    {
                        foreach (var b in buckets)
                        {
                            if (b == null) continue;
                            b.Cost = 0f;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Log.LogError($"[Customization] Failed to set Wall Paint costs: {ex.Message}");
                }

                try
                {
                    var floors = Resources.FindObjectsOfTypeAll<__Project__.Scripts.FloorPaintSystem.FloorSo>();
                    if (floors != null && floors.Length > 0)
                    {
                        foreach (var f in floors)
                        {
                            if (f == null) continue;
                            f.Cost = 0f;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Log.LogError($"[Customization] Failed to set Floor costs: {ex.Message}");
                }
            }
            // Debug Dump of all licenses for reference
            //  SupermarketSimArchipelago.LicenseUiHelper.DumpLicensesReference();
        }
    }

    [HarmonyPatch(typeof(__Project__.Scripts.Computer.Management.CustomizationTab.ChangeNameItem), "Start")]
    public static class ChangeNameItemStartPatch
    {
        [HarmonyPrefix]
        public static void Prefix(__Project__.Scripts.Computer.Management.CustomizationTab.ChangeNameItem __instance)
        {
            if (ArchipelagoConfig.FreeCustomizables && __instance != null)
            {
                __instance.m_Price = 0f;
            }
        }

        [HarmonyPostfix]
        public static void Postfix(__Project__.Scripts.Computer.Management.CustomizationTab.ChangeNameItem __instance)
        {
            if (ArchipelagoConfig.FreeCustomizables && __instance != null && __instance.m_MoneyText != null)
            {
                __instance.m_MoneyText.text = "$0";
            }
        }
    }

    [HarmonyPatch(typeof(__Project__.Scripts.Computer.Management.CustomizationTab.ChangeNameItem), "UpdatePurchaseButton")]
    public static class ChangeNameItemUpdatePurchaseButtonPatch
    {
        [HarmonyPrefix]
        public static void Prefix(ref float __0)
        {
            if (ArchipelagoConfig.FreeCustomizables)
            {
                __0 = 0f;
            }
        }

        [HarmonyPostfix]
        public static void Postfix(__Project__.Scripts.Computer.Management.CustomizationTab.ChangeNameItem __instance)
        {
            if (ArchipelagoConfig.FreeCustomizables && __instance != null && __instance.m_MoneyText != null)
            {
                __instance.m_MoneyText.text = "$0";
            }
        }
    }

    [HarmonyPatch(typeof(__Project__.Scripts.Computer.Management.CustomizationTab.DoorPlaceItem), "Start")]
    public static class DoorPlaceItemStartPatch
    {
        [HarmonyPrefix]
        public static void Prefix(__Project__.Scripts.Computer.Management.CustomizationTab.DoorPlaceItem __instance)
        {
            if (ArchipelagoConfig.FreeCustomizables && __instance != null)
            {
                __instance.m_Price = 0f;
            }
        }

        [HarmonyPostfix]
        public static void Postfix(__Project__.Scripts.Computer.Management.CustomizationTab.DoorPlaceItem __instance)
        {
            if (ArchipelagoConfig.FreeCustomizables && __instance != null && __instance.m_PriceText != null)
            {
                __instance.m_PriceText.text = "$0";
            }
        }
    }

    [HarmonyPatch(typeof(__Project__.Scripts.Computer.Management.CustomizationTab.DoorPlaceItem), "UpdatePurchaseButton")]
    public static class DoorPlaceItemUpdatePurchaseButtonPatch
    {
        [HarmonyPrefix]
        public static void Prefix(ref float __0)
        {
            if (ArchipelagoConfig.FreeCustomizables)
            {
                __0 = 0f;
            }
        }

        [HarmonyPostfix]
        public static void Postfix(__Project__.Scripts.Computer.Management.CustomizationTab.DoorPlaceItem __instance)
        {
            if (ArchipelagoConfig.FreeCustomizables && __instance != null && __instance.m_PriceText != null)
            {
                __instance.m_PriceText.text = "$0";
            }
        }
    }

    [HarmonyPatch(typeof(__Project__.Scripts.EntranceVariants.EntranceChangeItem), "Start")]
    public static class EntranceChangeItemStartPatch
    {
        [HarmonyPrefix]
        public static void Prefix(__Project__.Scripts.EntranceVariants.EntranceChangeItem __instance)
        {
            if (ArchipelagoConfig.FreeCustomizables && __instance != null)
            {
                __instance.m_Price = 0f;
            }
        }

        [HarmonyPostfix]
        public static void Postfix(__Project__.Scripts.EntranceVariants.EntranceChangeItem __instance)
        {
            if (ArchipelagoConfig.FreeCustomizables && __instance != null && __instance.m_PriceText != null)
            {
                __instance.m_PriceText.text = "$0";
            }
        }
    }

    [HarmonyPatch(typeof(__Project__.Scripts.EntranceVariants.EntranceChangeItem), "UpdatePurchaseButton")]
    public static class EntranceChangeItemUpdatePurchaseButtonPatch
    {
        [HarmonyPrefix]
        public static void Prefix(ref float __0)
        {
            if (ArchipelagoConfig.FreeCustomizables)
            {
                __0 = 0f;
            }
        }

        [HarmonyPostfix]
        public static void Postfix(__Project__.Scripts.EntranceVariants.EntranceChangeItem __instance)
        {
            if (ArchipelagoConfig.FreeCustomizables && __instance != null && __instance.m_PriceText != null)
            {
                __instance.m_PriceText.text = "$0";
            }
        }
    }

    [HarmonyPatch(typeof(EntranceChangeOverlay), "Initialize")]
    public static class EntranceChangeOverlayInitializePatch
    {
        [HarmonyPrefix]
        public static void Prefix(ref float price)
        {
            if (ArchipelagoConfig.FreeCustomizables)
            {
                price = 0f;
            }
        }
    }
}
