using HarmonyLib;
using UnityEngine;
using UnityEngine.Localization.Components;
using SupermarketSimArchipelago;

namespace SupermarketArchipelago
{
    // ==========================================
    // LEVEL REQUIREMENT & AP PROGRESSION GATE
    // ==========================================
    [HarmonyPatch(typeof(LoanItem), nameof(LoanItem.m_Locked), MethodType.Getter)]
    public class LoanLockedPropertyBypassPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(LoanItem __instance, ref bool __result)
        {
            if (!ArchipelagoConfig.EnableLoanLocks) return true;

            if (!ArchipelagoClient.HasLoanUnlock(__instance.LoanID))
            {
                __result = true;
                return false; 
            }


            return true;
        }
    }

    // ==========================================
    // UI LIVE HOOKS (TRIGGERS VISUAL OVERRIDES)
    // ==========================================
    [HarmonyPatch(typeof(LoanItem), nameof(LoanItem.Setup))]
    public class LoanSetupPatch
    {
        [HarmonyPostfix]
        public static void Postfix(LoanItem __instance)
        {
            LoanUiHelper.ForceLoanVisuals(__instance);
        }
    }

    [HarmonyPatch(typeof(LoanItem), nameof(LoanItem.CheckIfReachedRequiredLevel))]
    public class LoanCheckLevelPatch
    {
        [HarmonyPostfix]
        public static void Postfix(LoanItem __instance)
        {
            LoanUiHelper.ForceLoanVisuals(__instance);
        }
    }

    [HarmonyPatch(typeof(LoanItem), nameof(LoanItem.UpdateAvailableLayoutUI))]
    public class LoanUpdateAvailableUiPatch
    {
        [HarmonyPostfix]
        public static void Postfix(LoanItem __instance)
        {
            LoanUiHelper.ForceLoanVisuals(__instance);
        }
    }

    // ==========================================
    // CLICK SECURITY GUARD
    // ==========================================
    [HarmonyPatch(typeof(LoanItem), nameof(LoanItem.TakeLoan))]
    public class LoanTakeBlockerPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(LoanItem __instance)
        {
            if (!ArchipelagoConfig.EnableLoanLocks) return true;

            if (!ArchipelagoClient.HasLoanUnlock(__instance.LoanID))
                return false;
            return true;
        }
    }

    // ==========================================
    // UI VISUAL OVERRIDE HELPER
    // ==========================================
    public static class LoanUiHelper
    {
        public static void ForceLoanVisuals(LoanItem item)
        {
            if (item == null) return;
            if (!ArchipelagoConfig.EnableLoanLocks) return;
            if (item.Taken) return;

            bool isApunlocked = ArchipelagoClient.HasLoanUnlock(item.LoanID);

            if (!isApunlocked)
            {

                if (item.m_LockedLayout != null) item.m_LockedLayout.SetActive(true);
                if (item.m_AvailableLayout != null) item.m_AvailableLayout.SetActive(false);
                if (item.m_LoanButton != null) item.m_LoanButton.interactable = false;

                if (item.m_RequiredLevelText != null)
                {
                    item.m_RequiredLevelText.enabled = false;

                    var textMesh = item.m_RequiredLevelText.GetComponent<TMPro.TMP_Text>()
                                   ?? item.m_RequiredLevelText.GetComponentInChildren<TMPro.TMP_Text>();

                    if (textMesh != null)
                    {
                        textMesh.text = "Item required";
                        textMesh.color = Color.red;
                    }
                }
            }
            else
            {
                if (item.m_RequiredLevelText != null && !item.m_RequiredLevelText.enabled)
                {
                    item.m_RequiredLevelText.enabled = true;
                    item.m_RequiredLevelText.RefreshString();
                }


                bool levelReached = !item.m_Locked;

                if (levelReached)
                {
                    if (item.m_LockedLayout != null) item.m_LockedLayout.SetActive(false);
                    if (item.m_AvailableLayout != null) item.m_AvailableLayout.SetActive(true);
                    if (item.m_LoanButton != null) item.m_LoanButton.interactable = true;
                }
                else
                {
                    if (item.m_LockedLayout != null) item.m_LockedLayout.SetActive(true);
                    if (item.m_AvailableLayout != null) item.m_AvailableLayout.SetActive(false);
                    if (item.m_LoanButton != null) item.m_LoanButton.interactable = false;
                }
            }
        }
    }
}