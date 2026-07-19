using __Project__.Scripts.Computer.Management.Hiring_Tab;
using HarmonyLib;
using PG;
using System;
using UnityEngine;
using UnityEngine.Localization.Components;

namespace SupermarketArchipelago
{
    // ==========================================
    // UI ROOT INTERCEPTOR
    // ==========================================
    [HarmonyPatch(typeof(UnityEngine.UI.Selectable), nameof(UnityEngine.UI.Selectable.interactable), MethodType.Setter)]
    public class SelectableInteractablePatch
    {
        public static void Prefix(UnityEngine.UI.Selectable __instance, ref bool __0)
        {
            if (__instance == null) return;

            if (IsStaffButton(__instance, out bool hasItem, out bool isHired, out bool isBaker))
            {
                if (hasItem && !isHired)
                {
                    if (!isBaker)
                    {
                        __0 = true;
                    }
                }
                else if (!hasItem)
                {
                    __0 = false; 
                }
            }
        }

        private static bool IsStaffButton(UnityEngine.UI.Selectable sel, out bool hasItem, out bool isHired, out bool isBaker)
        {
            hasItem = false; isHired = false; isBaker = false;

            var c = sel.GetComponentInParent<CashierItem>();
            if (c != null && sel == c.m_HireButton) { hasItem = c.CashierId <= ArchipelagoClient.GetReceivedCashierCount(); isHired = c.Hired; return true; }

            var j = sel.GetComponentInParent<JanitorItem>();
            if (j != null && sel == j.m_HireButton) { hasItem = j.JanitorId <= ArchipelagoClient.GetReceivedJanitorCount(); isHired = j.Hired; return true; }

            var r = sel.GetComponentInParent<RestockerItem>();
            if (r != null && sel == r.m_HireButton) { hasItem = r.RestockerId <= ArchipelagoClient.GetReceivedRestockerCount(); isHired = r.Hired; return true; }

            var s = sel.GetComponentInParent<SecurityGuardItem>();
            if (s != null && sel == s.m_HireButton) { hasItem = s.HelperId <= ArchipelagoClient.GetReceivedSecurityCount(); isHired = s.Hired; return true; }

            var h = sel.GetComponentInParent<CustomerHelperItem>();
            if (h != null && sel == h.m_HireButton) { hasItem = h.HelperId <= ArchipelagoClient.GetReceivedHelperCount(); isHired = h.Hired; return true; }

            var ic = sel.GetComponentInParent<IceCreamHelperItem>();
            if (ic != null && sel == ic.m_HireButton) { hasItem = ic.ID <= ArchipelagoClient.GetReceivedIceCreamHelperCount(); isHired = ic.Hired; return true; }

            var b = sel.GetComponentInParent<BakerItem>();
            if (b != null && sel == b.m_HireButton)
            {
                hasItem = b.BakerId <= ArchipelagoClient.GetReceivedBakerCount();
                isHired = b.Hired;
                isBaker = true;
                return true;
            }

            return false;
        }
    }

    // ==========================================
    // CENTRAL UI MANAGER AND REFRESH MECHANISM
    // ==========================================
    public static class PersonalUiManager
    {
        public static void RefreshPersonalUI()
        {
            try
            {
                foreach (var item in UnityEngine.Object.FindObjectsOfType<CashierItem>())
                    item.UpdateRequiredStoreLevel(true);

                foreach (var item in UnityEngine.Object.FindObjectsOfType<JanitorItem>())
                    item.UpdateRequiredStoreLevel(true);

                foreach (var item in UnityEngine.Object.FindObjectsOfType<RestockerItem>())
                    item.UpdateRequiredStoreLevel(true);

                foreach (var item in UnityEngine.Object.FindObjectsOfType<SecurityGuardItem>())
                    item.UpdateRequiredStoreLevel(true);

                foreach (var item in UnityEngine.Object.FindObjectsOfType<CustomerHelperItem>())
                    item.UpdateRequiredStoreLevel(true);

                foreach (var item in UnityEngine.Object.FindObjectsOfType<IceCreamHelperItem>())
                    item.UpdateRequiredStoreLevel(true);
                var bakerSetupMethod = AccessTools.Method(typeof(BakerItem), "Setup");
                foreach (var item in UnityEngine.Object.FindObjectsOfType<BakerItem>())
                {
                    bakerSetupMethod?.Invoke(item, null);
                }
            }
            catch (Exception)
            {
            }
        }


        public static void ApplyStaffUi(bool hasItem, bool isHired, UnityEngine.UI.Button hireButton, TMPro.TMP_Text reqLevelText, LocalizeStringEvent reqLevelLoc, Color incColor, Color compColor, GameObject obj1 = null, GameObject obj2 = null, TMPro.TMP_Text objLevelText = null, LocalizeStringEvent objLevelLoc = null)
        {
            if (hireButton == null) return;

            if (obj1 != null) obj1.SetActive(false);
            if (obj2 != null) obj2.SetActive(false);
            if (reqLevelLoc != null) reqLevelLoc.enabled = false;
            if (objLevelLoc != null) objLevelLoc.enabled = false;

            if (isHired) return;

            TMPro.TMP_Text btnText = hireButton.GetComponentInChildren<TMPro.TMP_Text>();
            LocalizeStringEvent btnLoc = btnText != null ? (btnText.GetComponent<LocalizeStringEvent>() ?? hireButton.GetComponent<LocalizeStringEvent>()) : null;

            if (objLevelText != null)
            {
                objLevelText.gameObject.SetActive(true); objLevelText.text = "You need a Storage to hire"; objLevelText.color = incColor;
            }

            if (!hasItem)
            {
                hireButton.interactable = false;
                if (btnLoc != null) btnLoc.enabled = false;
                if (btnText != null) btnText.text = "Locked";
                if (reqLevelText != null) { reqLevelText.gameObject.SetActive(true); reqLevelText.text = "Item required"; reqLevelText.color = incColor; }
            }
            else
            {
                if (btnLoc != null) { btnLoc.enabled = true; btnLoc.RefreshString(); }
                if (reqLevelText != null) { reqLevelText.gameObject.SetActive(true); reqLevelText.text = "Item unlocked"; reqLevelText.color = compColor; }
            }
        }
    }

    // ==========================================
    // STAFF ITEM PATCHES
    // ==========================================

    [HarmonyPatch(typeof(CashierItem))]
    public class CashierPatches
    {
        [HarmonyPatch(nameof(CashierItem.CanHire))]
        [HarmonyPostfix]
        public static void CanHirePostfix(CashierItem __instance, ref bool __result)
        {
            if (__instance.CashierId <= ArchipelagoClient.GetReceivedCashierCount() && !__instance.Hired) __result = true;
        }

        [HarmonyPatch("UpdateRequiredStoreLevel", new Type[] { typeof(bool) })]
        [HarmonyPostfix]
        public static void UiPostfix(CashierItem __instance)
        {
            bool hasItem = __instance.CashierId <= ArchipelagoClient.GetReceivedCashierCount();
            __instance.m_HireLocked = !hasItem;

            if (hasItem && __instance.m_CashierSetup != null)
            {
                __instance.m_CashierSetup.RequiredStoreLevel = 0;
                __instance.m_CashierSetup.CheckoutGoalToUnlock = 0;
            }
            PersonalUiManager.ApplyStaffUi(hasItem, __instance.Hired, __instance.m_HireButton, __instance.m_RequiredStoreLevelText, __instance.m_LocalizedRequiredStoreLevelText, __instance.m_IncompletedRequirementColor, __instance.m_CompletedRequirementColor, __instance.m_CheckoutObjectiveText != null ? __instance.m_CheckoutObjectiveText.gameObject : null);
        }
    }

    [HarmonyPatch(typeof(JanitorItem))]
    public class JanitorPatches
    {
        [HarmonyPatch("CanHire")]
        [HarmonyPostfix]
        public static void CanHirePostfix(JanitorItem __instance, ref bool __result)
        {
            if (__instance.JanitorId <= ArchipelagoClient.GetReceivedJanitorCount() && !__instance.Hired) __result = true;
        }

        [HarmonyPatch("UpdateRequiredStoreLevel", new Type[] { typeof(bool) })]
        [HarmonyPostfix]
        public static void UiPostfix(JanitorItem __instance)
        {
            bool hasItem = __instance.JanitorId <= ArchipelagoClient.GetReceivedJanitorCount();
            __instance.m_HireLocked = !hasItem;
            __instance.m_JanitorSetup.RequiredStoreLevel = 0;

            if (hasItem && __instance.m_JanitorSetup != null)
            {
                __instance.m_JanitorSetup.RequiredStoreLevel = 0;

            }
            PersonalUiManager.ApplyStaffUi(hasItem, __instance.Hired, __instance.m_HireButton, __instance.m_RequiredStoreLevelText, __instance.m_LocalizedRequiredStoreLevelText, __instance.m_IncompletedRequirementColor, __instance.m_CompletedRequirementColor);
        }
    }

    [HarmonyPatch(typeof(RestockerItem))]
    public class RestockerPatches
    {
        [HarmonyPatch("CanHire")]
        [HarmonyPostfix]
        public static void CanHirePostfix(RestockerItem __instance, ref bool __result)
        {
            if (__instance.RestockerId <= ArchipelagoClient.GetReceivedRestockerCount() && !__instance.Hired) __result = true;
        }

        [HarmonyPatch("UpdateRequiredStoreLevel", new Type[] { typeof(bool) })]
        [HarmonyPostfix]
        public static void UiPostfix(RestockerItem __instance)
        {
            bool hasItem = __instance.RestockerId <= ArchipelagoClient.GetReceivedRestockerCount();
            __instance.m_HireLocked = !hasItem;

            if (hasItem && __instance.m_RestockerSetup != null)
            {
                __instance.m_RestockerSetup.RequiredStoreLevel = 0;
                __instance.m_RestockerSetup.RackGoalToUnlock = 0;

            }
            PersonalUiManager.ApplyStaffUi(hasItem, __instance.Hired, __instance.m_HireButton, __instance.m_RequiredStoreLevelText, __instance.m_LocalizedRequiredStoreLevelText, __instance.m_IncompletedRequirementColor, __instance.m_CompletedRequirementColor, __instance.m_StorageObjectiveText != null ? __instance.m_StorageObjectiveText.gameObject : null, __instance.m_RackObjectiveText != null ? __instance.m_RackObjectiveText.gameObject : null, __instance.m_StorageObjectiveText, __instance.m_LocalizedStorageObjectiveText);
        }
    }

    [HarmonyPatch(typeof(SecurityGuardItem))]
    public class SecurityPatches
    {
        [HarmonyPatch("CanHire")]
        [HarmonyPostfix]
        public static void CanHirePostfix(SecurityGuardItem __instance, ref bool __result)
        {
            if (__instance.HelperId <= ArchipelagoClient.GetReceivedSecurityCount() && !__instance.Hired) __result = true;
        }

        [HarmonyPatch("UpdateRequiredStoreLevel", new Type[] { typeof(bool) })]
        [HarmonyPostfix]
        public static void UiPostfix(SecurityGuardItem __instance)
        {
            bool hasItem = __instance.HelperId <= ArchipelagoClient.GetReceivedSecurityCount();
            __instance.m_HireLocked = !hasItem;

            if (hasItem && __instance.m_SecurityGuardSetup != null)
            {
                __instance.m_SecurityGuardSetup.RequiredStoreLevel = 0;

            }
            PersonalUiManager.ApplyStaffUi(hasItem, __instance.Hired, __instance.m_HireButton, __instance.m_RequiredStoreLevelText, __instance.m_LocalizedRequiredStoreLevelText, __instance.m_IncompletedRequirementColor, __instance.m_CompletedRequirementColor);
        }
    }

    [HarmonyPatch(typeof(CustomerHelperItem))]
    public class CustomerHelperPatches
    {
        [HarmonyPatch("CanHire")]
        [HarmonyPostfix]
        public static void CanHirePostfix(CustomerHelperItem __instance, ref bool __result)
        {
            if (__instance.HelperId <= ArchipelagoClient.GetReceivedHelperCount() && !__instance.Hired) __result = true;
        }

        [HarmonyPatch("UpdateRequiredStoreLevel", new Type[] { typeof(bool) })]
        [HarmonyPostfix]
        public static void UiPostfix(CustomerHelperItem __instance)
        {
            bool hasItem = __instance.HelperId <= ArchipelagoClient.GetReceivedHelperCount();
            __instance.m_HireLocked = !hasItem;

            if (hasItem && __instance.m_CustomerHelperSetup != null)
            {
                __instance.m_CustomerHelperSetup.RequiredStoreLevel = 0;
                __instance.m_CustomerHelperSetup.SelfCheckoutCountToUnlock = 0;

            }
            PersonalUiManager.ApplyStaffUi(hasItem, __instance.Hired, __instance.m_HireButton, __instance.m_RequiredStoreLevelText, __instance.m_LocalizedRequiredStoreLevelText, __instance.m_IncompletedRequirementColor, __instance.m_CompletedRequirementColor, __instance.m_CheckoutObjectiveText != null ? __instance.m_CheckoutObjectiveText.gameObject : null);
        }
    }

    [HarmonyPatch(typeof(IceCreamHelperItem))]
    public class IceCreamHelperPatches
    {
        [HarmonyPatch("CanHire")]
        [HarmonyPostfix]
        public static void CanHirePostfix(IceCreamHelperItem __instance, ref bool __result)
        {
            if (__instance.ID <= ArchipelagoClient.GetReceivedIceCreamHelperCount() && !__instance.Hired) __result = true;
        }

        [HarmonyPatch("UpdateRequiredStoreLevel", new Type[] { typeof(bool) })]
        [HarmonyPostfix]
        public static void UiPostfix(IceCreamHelperItem __instance)
        {
            bool hasItem = __instance.ID <= ArchipelagoClient.GetReceivedIceCreamHelperCount();
            __instance.m_HireLocked = !hasItem;

            if (hasItem && __instance.m_IceCreamHelperSetup != null)
            {
                __instance.m_IceCreamHelperSetup.RequiredStoreLevel = 0;

            }
            PersonalUiManager.ApplyStaffUi(hasItem, __instance.Hired, __instance.m_HireButton, __instance.m_RequiredStoreLevelText, __instance.m_LocalizedRequiredStoreLevelText, __instance.m_IncompletedRequirementColor, __instance.m_CompletedRequirementColor);
        }
    }


    [HarmonyPatch(typeof(BakerItem))]
    public class BakerPatches
    {
        [HarmonyPatch(nameof(BakerItem.CanHire))]
        [HarmonyPostfix]
        public static void CanHirePostfix(BakerItem __instance, ref bool __result)
        {
            bool hasItem = __instance.BakerId <= ArchipelagoClient.GetReceivedBakerCount();
            if (!hasItem)
            {
                __result = false;
            }
        }

        [HarmonyPatch("Setup")]
        [HarmonyPostfix]
        public static void SetupPostfix(BakerItem __instance) => UpdateBakerObjectives(__instance);

        [HarmonyPatch("UpdateOvenObjective")]
        [HarmonyPostfix]
        public static void UpdateOvenObjectivePostfix(BakerItem __instance) => UpdateBakerObjectives(__instance);

        [HarmonyPatch("UpdateBakeryDisplayObjective")]
        [HarmonyPostfix]
        public static void UpdateBakeryDisplayObjectivePostfix(BakerItem __instance) => UpdateBakerObjectives(__instance);

        private static void UpdateBakerObjectives(BakerItem __instance)
        {
            if (__instance == null) return;

            bool hasItem = __instance.BakerId <= ArchipelagoClient.GetReceivedBakerCount();
            __instance.m_HireLocked = !hasItem;

            if (__instance.Hired) return;

            if (__instance.m_LocalizedOvenObjectiveText != null)
            {
                __instance.m_LocalizedOvenObjectiveText.enabled = true;
                __instance.m_LocalizedOvenObjectiveText.RefreshString();
            }
            if (__instance.m_LocalizedBakeryDisplayObjectiveText != null)
            {
                __instance.m_LocalizedBakeryDisplayObjectiveText.enabled = true;
                __instance.m_LocalizedBakeryDisplayObjectiveText.RefreshString();
            }

            string vanillaOven = __instance.m_OvenObjectiveText != null ? __instance.m_OvenObjectiveText.text : "Oven 0/1";
            string vanillaDisplay = __instance.m_BakeryDisplayObjectiveText != null ? __instance.m_BakeryDisplayObjectiveText.text : "Display 0/1";

            
            if (__instance.m_LocalizedOvenObjectiveText != null) __instance.m_LocalizedOvenObjectiveText.enabled = false;
            if (__instance.m_LocalizedBakeryDisplayObjectiveText != null) __instance.m_LocalizedBakeryDisplayObjectiveText.enabled = false;

            if (__instance.m_OvenObjectiveText != null)
            {
                __instance.m_OvenObjectiveText.gameObject.SetActive(true);
                if (!hasItem)
                {
                    __instance.m_OvenObjectiveText.text = "Item required";
                    __instance.m_OvenObjectiveText.color = __instance.m_IncompletedRequirementColor;
                }
                else
                {
                    __instance.m_OvenObjectiveText.text = "Item unlocked";
                    __instance.m_OvenObjectiveText.color = __instance.m_CompletedRequirementColor;
                }
            }

            if (__instance.m_BakeryDisplayObjectiveText != null)
            {
                __instance.m_BakeryDisplayObjectiveText.gameObject.SetActive(true);

                bool ovenEnough = __instance.IsOvenCountEnough;
                bool displayEnough = __instance.IsBakeryDisplayCountEnough;
                        
                                        

                string ovenColorHex = ovenEnough ? ColorUtility.ToHtmlStringRGBA(__instance.m_CompletedRequirementColor) : ColorUtility.ToHtmlStringRGBA(__instance.m_IncompletedRequirementColor);
                string displayColorHex = displayEnough ? ColorUtility.ToHtmlStringRGBA(__instance.m_CompletedRequirementColor) : ColorUtility.ToHtmlStringRGBA(__instance.m_IncompletedRequirementColor);

                __instance.m_BakeryDisplayObjectiveText.text = $"<color=#{ovenColorHex}>{vanillaOven}</color>\n<color=#{displayColorHex}>{vanillaDisplay}</color>";
            }

            if (!hasItem || !__instance.IsOvenCountEnough || !__instance.IsBakeryDisplayCountEnough)
            {
                if (__instance.m_HireButton != null) __instance.m_HireButton.interactable = false;
                TMPro.TMP_Text btnText = __instance.m_HireButton?.GetComponentInChildren<TMPro.TMP_Text>();
                if (btnText != null)
                {
                    var btnLoc = __instance.m_HireButton.GetComponent<LocalizeStringEvent>() ?? btnText.GetComponent<LocalizeStringEvent>();
                    if (btnLoc != null) btnLoc.enabled = false;
                    btnText.text = "Locked";
                }
            } else
            {
                if (__instance.m_HireButton != null) __instance.m_HireButton.interactable = true;
                TMPro.TMP_Text btnText = __instance.m_HireButton?.GetComponentInChildren<TMPro.TMP_Text>();
                if (btnText != null)
                {
                    var btnLoc = __instance.m_HireButton.GetComponent<LocalizeStringEvent>() ?? btnText.GetComponent<LocalizeStringEvent>();
                    if (btnLoc != null) { btnLoc.enabled = true; btnLoc.RefreshString(); }
                }
            }
        }
    }
}