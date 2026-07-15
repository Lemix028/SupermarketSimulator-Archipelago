using HarmonyLib;
using SupermarketArchipelago;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Components;
using TMPro;
using JetBrains.Annotations;

namespace SupermarketSimArchipelago
{
    [HarmonyPatch(typeof(FurnitureSalesItem), nameof(FurnitureSalesItem.AddToCart))]
    public class FurnitureAddToCartPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(FurnitureSalesItem __instance)
        {
            int furnitureID = __instance.ProductId;

            if (!ArchipelagoClient.CheckIncomingFurniture(furnitureID))
                return false;

            StoreLevelBypassPatch.UseFakeLevel = true;
            return true;
        }

        [HarmonyPostfix]
        public static void Postfix()
        {
            StoreLevelBypassPatch.UseFakeLevel = false;
        }
    }

    // ==========================================================
    // UI LIFECYCLE PATCHES
    // ==========================================================

    [HarmonyPatch(typeof(FurnitureSalesItem), nameof(FurnitureSalesItem.Setup))]
    public class FurnitureSetupPatch
    {
        [HarmonyPrefix] public static void Prefix() => StoreLevelBypassPatch.UseFakeLevel = true;

        [HarmonyPostfix]
        public static void Postfix(FurnitureSalesItem __instance)
        {
            StoreLevelBypassPatch.UseFakeLevel = false;
            FurnitureUiHelper.ForceArchipelagoVisuals(__instance);
        }
    }

    [HarmonyPatch(typeof(FurnitureSalesItem), nameof(FurnitureSalesItem.ReSetup))]
    public class FurnitureReSetupPatch
    {
        [HarmonyPrefix] public static void Prefix() => StoreLevelBypassPatch.UseFakeLevel = true;

        [HarmonyPostfix]
        public static void Postfix(FurnitureSalesItem __instance)
        {
            StoreLevelBypassPatch.UseFakeLevel = false;
            FurnitureUiHelper.ForceArchipelagoVisuals(__instance);
        }
    }

    // ==========================================
    // FURNITURE UI INJECTION VISUAL HELPER
    // ==========================================
    public static class FurnitureUiHelper
    {
        public static void ForceArchipelagoVisuals(FurnitureSalesItem item)
        {
    

            if (item == null) return;

            int furnitureID = item.ProductId;
            bool hasItem = ArchipelagoClient.CheckIncomingFurniture(furnitureID);

            var allButtons = item.GetComponentsInChildren<Button>(true);
            var allLocalizers = item.GetComponentsInChildren<LocalizeStringEvent>(true);
            if (!hasItem)
            {
                foreach (var localizer in allLocalizers)
                {
                    if (localizer != null)
                    {
                        localizer.OnUpdateString.RemoveAllListeners();
                        localizer.enabled = false;
                    }
                }
            }

            foreach (var btn in allButtons)
            {
                if (btn == null) continue;

                if (!hasItem)
                {
                    btn.interactable = false;

                    var btnTMPs = btn.GetComponentsInChildren<TMP_Text>(true);
                    foreach (var t in btnTMPs) if (t != null) t.text = "LOCKED";

                    var btnTexts = btn.GetComponentsInChildren<Text>(true);
                    foreach (var t in btnTexts) if (t != null) t.text = "LOCKED";
                }
                else
                {
                    btn.interactable = true;

                    var btnLocalizers = btn.GetComponentsInChildren<LocalizeStringEvent>(true);
                    foreach (var loc in btnLocalizers)
                    {
                        if (loc != null)
                        {
                            loc.enabled = true;
                            loc.RefreshString();
                        }
                    }
                }
            }
        }
    }
}