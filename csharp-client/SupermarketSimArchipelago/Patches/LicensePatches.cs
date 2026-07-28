using HarmonyLib;
using SupermarketArchipelago;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SupermarketSimArchipelago
{

    [HarmonyPatch(typeof(LicenseItem), nameof(LicenseItem.Purchase))]
    public class LicensePurchasePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(LicenseItem __instance)
        {
            int licenseID = __instance.LicenseID;

            if (!ArchipelagoClient.CheckIncomingLicense(licenseID))
            {
                Plugin.Log.LogWarning($"Purchase bLocked! Missing item for License ID {licenseID}.");
                return false;
            }

            StoreLevelBypassPatch.UseFakeLevel = true;
            return true;
        }

        [HarmonyPostfix]
        public static void Postfix(LicenseItem __instance)
        {
            StoreLevelBypassPatch.UseFakeLevel = false;
            GoalHandler.CheckLicensesGoal();

            if (__instance != null)
            {
                int licenseID = __instance.LicenseID;
                if (!ArchipelagoConfig.IsDefaultLicense(licenseID))
                {
                    int locationID = ArchipelagoIdHelper.FromLicensePurchase(licenseID);
                    ArchipelagoClient.SendLocation(locationID);
                }
            }
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
        public static void DumpLicensesReference()
        {
            try
            {
                string path = System.IO.Path.Combine(BepInEx.Paths.BepInExRootPath, "Archipelago_Licenses_Reference.txt");

                var manager = UnityEngine.Object.FindObjectOfType<ProductLicenseManager>();
                if (manager == null || manager.Licenses == null) return;

                List<string> lines = new List<string>();
                lines.Add("=== SUPERMARKET SIMULATOR ARCHIPELAGO LICENSE REFERENCE ===");
                lines.Add("");

                foreach (var lic in manager.Licenses)
                {
                    if (lic == null) continue;
                    int id = lic.ID;
                    string name = $"License {id}";

                    var productNames = new List<string>();
                    if (lic.Products != null)
                    {
                        foreach (var prod in lic.Products)
                        {
                            if (prod == null) continue;
                            string pName = prod.LocalizedName != null ? prod.LocalizedName.GetLocalizedString() : null;
                            if (string.IsNullOrEmpty(pName)) pName = prod.ProductName;
                            if (!string.IsNullOrEmpty(pName)) productNames.Add(pName);
                        }
                    }
                    string localizedProducts = productNames.Count > 0 ? string.Join(", ", productNames) : "No Products";

                    lines.Add($"- {name}: {localizedProducts} (Required Store Level: {lic.RequiredPlayerLevel}, Purchase Cost: ${lic.PurchasingCost})");
                }

                System.IO.File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Failed to dump license reference: {ex.Message}");
            }
        }

        public static string GetLocalizedLicenseName(int gameID)
        {
            try
            {
                var manager = UnityEngine.Object.FindObjectOfType<ProductLicenseManager>();
                if (manager != null && manager.Licenses != null)
                {
                    foreach (var lic in manager.Licenses)
                    {
                        if (lic != null && lic.ID == gameID)
                        {
                            var productNames = new List<string>();
                            if (lic.Products != null)
                            {
                                foreach (var prod in lic.Products)
                                {
                                    if (prod == null) continue;
                                    string pName = prod.LocalizedName != null ? prod.LocalizedName.GetLocalizedString() : null;
                                    if (string.IsNullOrEmpty(pName)) pName = prod.ProductName;
                                    if (!string.IsNullOrEmpty(pName)) productNames.Add(pName);
                                }
                            }
                            if (productNames.Count > 0)
                            {
                                return string.Join(", ", productNames);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[LicenseName] Failed to get localized license name for {gameID}: {ex.Message}");
            }
            return null;
        }

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
                if (item.m_PurchaseButtonText != null) item.m_PurchaseButtonText.text = "Locked";


                if (item.m_RequiredLevelText != null)
                {
                    item.m_RequiredLevelText.gameObject.SetActive(true);
                    item.m_RequiredLevelText.text = "Item required";
                    item.m_RequiredLevelText.color = item.m_NotReachedRequiredStoreLevelColor;
                }
            } else
            {
                RestorePurchaseButtonText(item);

                if (item.m_RequiredLevelText != null)
                {
                    item.m_RequiredLevelText.gameObject.SetActive(true);
                    item.m_RequiredLevelText.text = "Item unlocked";
                    item.m_ReachedRequiredStoreLevelColor = Color.green; // Set color to green to be absolutely clear
                    item.m_RequiredLevelText.color = item.m_ReachedRequiredStoreLevelColor;
                }
            }
        }

        private static void RestorePurchaseButtonText(LicenseItem item)
        {
            if (item?.m_PurchaseButtonText == null) return;

            UIHelper.RefreshLocalization(item.m_PurchaseButtonText);

            // Localization can update on a later frame. Do not leave the
            // Archipelago override visible while the license is already usable.
            if (string.Equals(item.m_PurchaseButtonText.text, "Locked", StringComparison.OrdinalIgnoreCase))
            {
                item.m_PurchaseButtonText.text = "Buy";
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

    [HarmonyPatch(typeof(LicensesTab), "OnEnable")]
    public class LicensesTabOnEnablePatch
    {
        [HarmonyPostfix]
        public static void Postfix(LicensesTab __instance)
        {
            if (__instance == null || __instance.m_LicenseItems == null) return;
            foreach (var item in __instance.m_LicenseItems)
            {
                if (item != null)
                    LicenseUiHelper.ForceArchipelagoVisuals(item);
            }
            try
            {
                __instance.ApplyFiltersAndSort();
            }
            catch { }
        }
    }
}
