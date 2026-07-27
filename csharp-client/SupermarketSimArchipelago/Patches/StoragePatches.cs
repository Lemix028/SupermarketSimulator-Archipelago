using HarmonyLib;
using UnityEngine;
using UnityEngine.Localization.Components;
using SupermarketSimArchipelago; 

namespace SupermarketArchipelago
{
    // ==========================================
    // Level & Section Requirement Bypass for Storage Upgrades
    // ==========================================
    [HarmonyPatch(typeof(StorageSectionItem), nameof(StorageSectionItem.m_ReachedRequiredStoreLevel), MethodType.Getter)]
    public class StorageSectionLevelBypassPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(StorageSectionItem __instance, ref bool __result)
        {
            if (!ArchipelagoConfig.EnableStorageLocks) return true;

            int unlockedCount = ArchipelagoClient.GetReceivedStorageCount();

            if (__instance.ID <= unlockedCount)
            {
                __result = true;
                return false;
            }

            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(StorageSectionItem), nameof(StorageSectionItem.m_ReachedRequiredSection), MethodType.Getter)]
    public class StorageSectionBypassPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(StorageSectionItem __instance, ref bool __result)
        {
            if (!ArchipelagoConfig.EnableStorageLocks) return true;

            if (__instance.ID == 1)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }

    // ==========================================
    // Override Storage Purchase to Check for Archipelago Unlocks
    // ==========================================
    [HarmonyPatch(typeof(StorageSectionItem), nameof(StorageSectionItem.Purchase))]
    public class StoragePurchaseBlockerPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(StorageSectionItem __instance)
        {
            if (ArchipelagoConfig.EnableStorageLocks)
            {
                int unlockedCount = ArchipelagoClient.GetReceivedStorageCount();

                if (__instance.ID > unlockedCount)
                    return false;
            }

            StoreLevelBypassPatch.UseFakeLevel = true;
            return true;
        }

        [HarmonyPostfix]
        public static void Postfix(StorageSectionItem __instance)
        {
            StoreLevelBypassPatch.UseFakeLevel = false;

            if (ArchipelagoConfig.EnableStorageLocks)
            {
                long locationId = ArchipelagoIdHelper.FromStorageUpgrade(__instance.ID);
                ArchipelagoClient.SendLocation(locationId);
            }
        }
    }

    // ==========================================
    // UI CORE HOOKS (WITH TEMPORARY LEVEL SPOOFING)
    // ==========================================
    [HarmonyPatch(typeof(StorageSectionItem), nameof(StorageSectionItem.CheckRequirements))]
    public class StorageCheckRequirementsPatch
    {
        [HarmonyPrefix]
        public static void Prefix(StorageSectionItem __instance)
        {
            if (!ArchipelagoConfig.EnableStorageLocks) return;

            int unlockedCount = ArchipelagoClient.GetReceivedStorageCount();
           
            if (__instance.ID <= unlockedCount)
            {
                StoreLevelBypassPatch.UseFakeLevel = true;
            }
        }

        [HarmonyPostfix]
        public static void Postfix(StorageSectionItem __instance)
        {
            StoreLevelBypassPatch.UseFakeLevel = false; 
            StorageUiHelper.ForceStorageVisuals(__instance);
        }
    }

    [HarmonyPatch(typeof(StorageSectionItem), nameof(StorageSectionItem.UpdatePurchaseButton))]
    public class StorageButtonAffordabilityPatch
    {
        [HarmonyPrefix]
        public static void Prefix(StorageSectionItem __instance)
        {
            if (!ArchipelagoConfig.EnableStorageLocks) return;

            int unlockedCount = ArchipelagoClient.GetReceivedStorageCount();
            if (__instance.ID <= unlockedCount)
            {
                StoreLevelBypassPatch.UseFakeLevel = true;
            }
        }

        [HarmonyPostfix]
        public static void Postfix(StorageSectionItem __instance)
        {
            StoreLevelBypassPatch.UseFakeLevel = false; 
            StorageUiHelper.ForceStorageVisuals(__instance);
        }
    }

    // ==========================================
    // UI HELPER FOR STORAGE UPGRADES
    // ==========================================
    public static class StorageUiHelper
    {
        public static void ForceStorageVisuals(StorageSectionItem item)
        {
            if (item == null) return;
            if (!ArchipelagoConfig.EnableStorageLocks) return;

            int unlockedCount = ArchipelagoClient.GetReceivedStorageCount();
            bool isApunlocked = item.ID <= unlockedCount;
            bool isSectionRequirementMet = (item.ID == 1) || item.m_ReachedRequiredSection;

            if (item.m_RequiredLevelLocalizedText != null)
            {
                item.m_RequiredLevelLocalizedText.OnUpdateString.RemoveAllListeners();
                item.m_RequiredLevelLocalizedText.enabled = false;
            }

            if (item.m_RequiredSectionLocalizedText != null)
            {
                item.m_RequiredSectionLocalizedText.OnUpdateString.RemoveAllListeners();
                item.m_RequiredSectionLocalizedText.enabled = false;
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
                    item.m_RequiredSectionText.gameObject.SetActive(true);
                    item.m_RequiredSectionText.text = "Previous Storage required";
                }
            }
            else
            {

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