using HarmonyLib;
using SupermarketArchipelago;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupermarketSimArchipelago
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

    [HarmonyPatch(typeof(LicenseItem), "UpdatePurchaseButton")]
    public class LicenseButtonAffordabilityPatch
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            // Enable fake level during UI calculation so it checks money instead of level
            StoreLevelBypassPatch.UseFakeLevel = true;
        }

        [HarmonyPostfix]
        public static void Postfix(LicenseItem __instance)
        {
            StoreLevelBypassPatch.UseFakeLevel = false;
            LicenseUiHelper.ForceArchipelagoVisuals(__instance);
        }
    }

    //UI ======

    public static class LicenseUiHelper
    {
        public static void ForceArchipelagoVisuals(LicenseItem item)
        {
            if (item == null) return;
            if (item.m_IsPurchased == true) return;
            bool hasItem = ArchipelagoClient.CheckIncomingLicense(item.LicenseID);

            if (item.m_RequiredLevelLocalizedText != null)
            {
                item.m_RequiredLevelLocalizedText.OnUpdateString.RemoveAllListeners();
                item.m_RequiredLevelLocalizedText.enabled = false;

            }
            if (!hasItem)
            {
                if (item.m_PurchaseButton != null) item.m_PurchaseButton.interactable = false;
                if (item.m_PurchaseButtonText != null) item.m_PurchaseButtonText.text = "LOCKED";


                if (item.m_RequiredLevelText != null)
                {
                    item.m_RequiredLevelText.gameObject.SetActive(true);
                    item.m_RequiredLevelText.text = "Item required";
                    item.m_RequiredLevelText.color = item.m_NotReachedRequiredStoreLevelColor;
                }
            } else
            {


                if (item.m_RequiredLevelText != null)
                {
                    item.m_RequiredLevelText.gameObject.SetActive(true);
                    item.m_RequiredLevelText.text = "Item unlocked";
                    item.m_RequiredLevelText.color = item.m_ReachedRequiredStoreLevelColor;
                }
            }
        }
    }

    [HarmonyPatch(typeof(LicenseItem), nameof(LicenseItem.RefreshFromState))]
    public class LicenseUiVisualPatch
    {
        [HarmonyPrefix] public static void Prefix() => StoreLevelBypassPatch.UseFakeLevel = true;

        [HarmonyPostfix]
        public static void Postfix(LicenseItem __instance)
        {
            StoreLevelBypassPatch.UseFakeLevel = false;
            LicenseUiHelper.ForceArchipelagoVisuals(__instance);
        }
    }

    [HarmonyPatch(typeof(LicenseItem), "CheckIfReachedRequiredLevel")]
    public class LicenseCheckLevelPatch
    {
        [HarmonyPostfix]
        public static void Postfix(LicenseItem __instance)
        {
            // Intercept the exact method that calculates the level text strings
            LicenseUiHelper.ForceArchipelagoVisuals(__instance);
        }
    }



}
