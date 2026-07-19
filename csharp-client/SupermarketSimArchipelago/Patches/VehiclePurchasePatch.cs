using HarmonyLib;
using SupermarketArchipelago;
using System;
using UnityEngine;
using UnityEngine.Localization.Components;

namespace SupermarketSimArchipelago
{
    [HarmonyPatch(typeof(VehicleSaleItem), nameof(VehicleSaleItem.Purchase))]
    public class VehiclePatch
    {

        [HarmonyPatch(typeof(VehicleSaleItem), nameof(VehicleSaleItem.Purchase))]
        public class VehiclePurchasePatch
        {
            [HarmonyPrefix]
            public static bool Prefix(VehicleSaleItem __instance)
            {
                int vehicleID = __instance.VehicleLevel;

                if (!ArchipelagoClient.CheckIncomingVehicle(vehicleID))
                    return false;

                if (MoneyManager.Instance == null || MoneyManager.Instance.Money < __instance.m_Price)
                    return false;

                try
                {
                    MoneyManager.Instance.MoneyTransition(-__instance.m_Price, MoneyManager.TransitionType.UPGRADE_COSTS);

                    __instance.IsPurchased = true;

                    __instance.Purchased();

                    __instance.Purchase_Network();

                }
                catch (Exception ex)
                {
                    Plugin.Log.LogError($"Critical error during custom vehicle purchase handler: {ex}");
                }

                return false;
            }
        }

        [HarmonyPrefix]
        public static bool Prefix(VehicleSaleItem __instance)
        {
            int vehicleID = __instance.VehicleLevel;

            if (!ArchipelagoClient.CheckIncomingVehicle(vehicleID))
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


    [HarmonyPatch(typeof(VehicleSaleItem), nameof(VehicleSaleItem.IsPurchaseAvailable))]
    public class VehiclePurchaseAvailablePatch
    {
        [HarmonyPostfix]
        public static void Postfix(VehicleSaleItem __instance, ref bool __result)
        {
            int vehicleID = __instance.VehicleLevel;

            if (ArchipelagoClient.CheckIncomingVehicle(vehicleID))
            {
                __result = true;
            }
            else
            {
                __result = false;
            }
        }
    }

    // ==========================================================
    // UI LIFECYCLE PATCHES
    // ==========================================================

    [HarmonyPatch(typeof(VehicleSaleItem), nameof(VehicleSaleItem.Setup))]
    public class VehicleSetupPatch
    {
        [HarmonyPrefix] public static void Prefix() => StoreLevelBypassPatch.UseFakeLevel = true;
        [HarmonyPostfix]
        public static void Postfix(VehicleSaleItem __instance)
        {
            StoreLevelBypassPatch.UseFakeLevel = false;
            VehicleUiHelper.ForceArchipelagoVisuals(__instance);
        }
    }

    [HarmonyPatch(typeof(VehicleSaleItem), nameof(VehicleSaleItem.OnEnable))]
    public class VehicleOnEnablePatch
    {
        [HarmonyPrefix] public static void Prefix() => StoreLevelBypassPatch.UseFakeLevel = true;
        [HarmonyPostfix]
        public static void Postfix(VehicleSaleItem __instance)
        {
            StoreLevelBypassPatch.UseFakeLevel = false;
            VehicleUiHelper.ForceArchipelagoVisuals(__instance);
        }
    }

    [HarmonyPatch(typeof(VehicleSaleItem), nameof(VehicleSaleItem.ResetVehicle))]
    public class VehicleResetPatch
    {
        [HarmonyPrefix] public static void Prefix() => StoreLevelBypassPatch.UseFakeLevel = true;
        [HarmonyPostfix]
        public static void Postfix(VehicleSaleItem __instance)
        {
            StoreLevelBypassPatch.UseFakeLevel = false;
            VehicleUiHelper.ForceArchipelagoVisuals(__instance);
        }
    }

    [HarmonyPatch(typeof(VehicleSaleItem), "UpdateButtonInteraction")]
    public class VehicleButtonInteractionPatch
    {
        [HarmonyPrefix] public static void Prefix() => StoreLevelBypassPatch.UseFakeLevel = true;
        [HarmonyPostfix]
        public static void Postfix(VehicleSaleItem __instance)
        {
            StoreLevelBypassPatch.UseFakeLevel = false;
            VehicleUiHelper.ForceArchipelagoVisuals(__instance);
        }
    }


    // ==========================================
    // VEHICLE UI INJECTION VISUAL HELPER
    // ==========================================
    public static class VehicleUiHelper
    {
        public static void ForceArchipelagoVisuals(VehicleSaleItem item)
        {
            if (item == null) return;
            if (item.IsPurchased) return;

            int vehicleID = item.VehicleLevel;
            bool hasItem = ArchipelagoClient.CheckIncomingVehicle(vehicleID);

            item.m_RequiredLevel = 0;
            item.IsUnlocked = hasItem;

            TMPro.TMP_Text buttonText = null;
            if (item.m_PurchaseButton != null)
            {
                buttonText = item.m_PurchaseButton.GetComponentInChildren<TMPro.TMP_Text>(true);
            }

            DisableLocalization(item.m_RequiredStoreText);
            DisableLocalization(item.m_RequiredStoreLevelText);
            if (buttonText != null) DisableLocalization(buttonText);

            if (!hasItem)
            {
                // --- Locked STATE ---
                if (item.m_PurchaseButton != null) item.m_PurchaseButton.interactable = false;
                if (buttonText != null) buttonText.text = "Locked";

                if (item.m_RequiredStoreText != null)
                {
                    item.m_RequiredStoreText.gameObject.SetActive(true);
                    item.m_RequiredStoreText.text = "Item required";
                    item.m_RequiredStoreText.color = item.m_False;
                }

                if (item.m_RequiredStoreLevelText != null)
                {
                    item.m_RequiredStoreLevelText.gameObject.SetActive(false);
                }
            }
            else
            {
                // --- unlocked / BUYABLE STATE ---
                if (item.m_RequiredStoreText != null) item.m_RequiredStoreText.gameObject.SetActive(false);
                if (item.m_RequiredStoreLevelText != null) item.m_RequiredStoreLevelText.gameObject.SetActive(false);

                if (item.m_PurchaseButton != null)
                {
                    bool canAfford = MoneyManager.Instance != null && MoneyManager.Instance.Money >= item.m_Price;
                    item.m_PurchaseButton.interactable = canAfford;
                }

                if (buttonText != null && buttonText.text == "Locked")
                {
                    var localizer = buttonText.GetComponent<LocalizeStringEvent>() ?? buttonText.transform.parent?.GetComponent<LocalizeStringEvent>();
                    if (localizer != null)
                    {
                        localizer.enabled = true;
                        localizer.RefreshString();
                    }
                    else
                    {
                        buttonText.text = "Buy";
                    }
                }
            }
        }

        private static void DisableLocalization(TMPro.TMP_Text textComponent)
        {
            if (textComponent == null) return;
            var localizer = textComponent.GetComponent<LocalizeStringEvent>() ?? textComponent.transform.parent?.GetComponent<LocalizeStringEvent>();
            if (localizer != null)
            {
                localizer.OnUpdateString.RemoveAllListeners();
                localizer.enabled = false;
            }
        }
    }
}