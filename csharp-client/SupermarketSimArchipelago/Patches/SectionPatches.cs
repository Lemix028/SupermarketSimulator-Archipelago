using HarmonyLib;
using UnityEngine;
using UnityEngine.Localization.Components;
using SupermarketSimArchipelago; // Access to StoreLevelBypassPatch

namespace SupermarketArchipelago
{
    // ==========================================
    // Level & Section Requirement Bypass for Growth Upgrades
    // ==========================================
    [HarmonyPatch(typeof(GrowthSectionItem), nameof(GrowthSectionItem.m_ReachedRequiredStoreLevel), MethodType.Getter)]
    public class UpgradeLevelBypassPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(GrowthSectionItem __instance, ref bool __result)
        {
            int unlockedCount = ArchipelagoClient.GetReceivedSectionCount();

            if (__instance.ID <= unlockedCount)
            {
                __result = true;
                return false;
            }

            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(GrowthSectionItem), nameof(GrowthSectionItem.m_ReachedRequiredSection), MethodType.Getter)]
    public class UpgradeSectionBypassPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(GrowthSectionItem __instance, ref bool __result)
        {
            if (__instance.ID == 1)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }

    // ==========================================
    // Override Growth Upgrade Purchase to Check for Archipelago Unlocks
    // ==========================================
    [HarmonyPatch(typeof(GrowthSectionItem), nameof(GrowthSectionItem.Purchase))]
    public class UpgradePurchaseBlockerPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(GrowthSectionItem __instance, out bool __state)
        {
            int unlockedCount = ArchipelagoClient.GetReceivedSectionCount();
            __state = __instance.ID <= unlockedCount;

            if (!__state)
                return false;

            StoreLevelBypassPatch.UseFakeLevel = true;
            return true;
        }

        [HarmonyPostfix]
        public static void Postfix(GrowthSectionItem __instance, bool __state)
        {
            StoreLevelBypassPatch.UseFakeLevel = false;
            if (__state && ArchipelagoConfig.EnableSectionLocations)
            {
                long locationId = ArchipelagoIdHelper.FromSectionUpgrade(__instance.ID);
                ArchipelagoClient.SendLocation(locationId);
            }
        }
    }

    // ==========================================
    // UI
    // ==========================================
    [HarmonyPatch(typeof(GrowthSectionItem), nameof(GrowthSectionItem.CheckRequirements))]
    public class UpgradeCheckRequirementsPatch
    {
        [HarmonyPostfix]
        public static void Postfix(GrowthSectionItem __instance)
        {
            UpgradeUiHelper.ForceUpgradeVisuals(__instance);
        }
    }

    [HarmonyPatch(typeof(GrowthSectionItem), "UpdatePurchaseButton")]
    public class UpgradeButtonAffordabilityPatch
    {
        [HarmonyPostfix]
        public static void Postfix(GrowthSectionItem __instance)
        {
            UpgradeUiHelper.ForceUpgradeVisuals(__instance);
        }
    }

    public static class UpgradeUiHelper
    {
        public static void ForceUpgradeVisuals(GrowthSectionItem item)
        {
            if (item == null) return;

            int unlockedCount = ArchipelagoClient.GetReceivedSectionCount();
            bool isApunlocked = item.ID <= unlockedCount;
            bool isSectionRequirementMet = (item.ID == 1) || item.m_ReachedRequiredSection;
            bool canAfford = MoneyManager.Instance != null
                && item.m_Section != null
                && MoneyManager.Instance.Money >= item.m_Section.Cost;

            if (item.m_RequiredLevelLocalizedText != null)
            {
                item.m_RequiredLevelLocalizedText.OnUpdateString.RemoveAllListeners();
                item.m_RequiredLevelLocalizedText.enabled = false;
            }

            TMPro.TMP_Text btnText = item.m_PurchaseButton != null ? item.m_PurchaseButton.GetComponentInChildren<TMPro.TMP_Text>() : null;
            LocalizeStringEvent btnLoc = btnText != null ? (btnText.GetComponent<LocalizeStringEvent>() ?? item.m_PurchaseButton.GetComponent<LocalizeStringEvent>()) : null;

            if (!isApunlocked)
            {
                if (item.m_PurchaseButton != null) item.m_PurchaseButton.interactable = false;

                if (btnLoc != null) btnLoc.enabled = false;
                if (btnText != null) btnText.text = "Locked";

                if (item.m_RequiredLevelText != null)
                {
                    item.m_RequiredLevelText.gameObject.SetActive(true);
                    item.m_RequiredLevelText.text = "Item required";
                    item.m_RequiredLevelText.color = item.m_NotReachedRequiredStoreLevelColor;
                }
                if (item.m_RequiredSectionText != null) item.m_RequiredSectionText.gameObject.SetActive(false);
            }
            else if (!isSectionRequirementMet)
            {
                if (item.m_PurchaseButton != null) item.m_PurchaseButton.interactable = false;

                if (btnLoc != null) btnLoc.enabled = false;
                if (btnText != null) btnText.text = "Locked";

                if (item.m_RequiredLevelText != null)
                {
                    item.m_RequiredLevelText.gameObject.SetActive(true);
                    item.m_RequiredLevelText.text = "Item unlocked";
                    item.m_RequiredLevelText.color = item.m_ReachedRequiredStoreLevelColor;
                }

                if (item.m_RequiredSectionText != null)
                {
                    var sectionLoc = item.m_RequiredSectionText.GetComponent<LocalizeStringEvent>();
                    if (sectionLoc != null) sectionLoc.enabled = false;

                    item.m_RequiredSectionText.gameObject.SetActive(true);
                    item.m_RequiredSectionText.text = "Previous Section required";
                }
            }
            else
            {
                if (item.m_PurchaseButton != null)
                    item.m_PurchaseButton.interactable = canAfford;

                if (btnLoc != null)
                {
                    btnLoc.enabled = true;
                    btnLoc.RefreshString();
                }

                if (item.m_RequiredLevelText != null)
                {
                    item.m_RequiredLevelText.gameObject.SetActive(true);
                    item.m_RequiredLevelText.text = "Item unlocked";
                    item.m_RequiredLevelText.color = item.m_ReachedRequiredStoreLevelColor;
                }
                if (item.m_RequiredSectionText != null) item.m_RequiredSectionText.gameObject.SetActive(false);
            }
        }
    }
}
