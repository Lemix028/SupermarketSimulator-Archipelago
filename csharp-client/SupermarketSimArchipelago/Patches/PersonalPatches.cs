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

            if (IsStaffButton(__instance, out bool hasItem, out bool isHired))
            {
                if (hasItem && !isHired)
                {
                    __0 = true;
                }
                else if (!hasItem)
                {
                    __0 = false;
                }
            }
        }

        private static bool IsStaffButton(UnityEngine.UI.Selectable sel, out bool hasItem, out bool isHired)
        {
            hasItem = false; isHired = false;

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

            foreach (var item in UnityEngine.Object.FindObjectsOfType<CashierItem>())
                RefreshItem(item, value => value.UpdateRequiredStoreLevel(true));
            foreach (var item in UnityEngine.Object.FindObjectsOfType<JanitorItem>())
                RefreshItem(item, value => value.UpdateRequiredStoreLevel(true));
            foreach (var item in UnityEngine.Object.FindObjectsOfType<RestockerItem>())
                RefreshItem(item, value => value.UpdateRequiredStoreLevel(true));
            foreach (var item in UnityEngine.Object.FindObjectsOfType<SecurityGuardItem>())
                RefreshItem(item, value => value.UpdateRequiredStoreLevel(true));
            foreach (var item in UnityEngine.Object.FindObjectsOfType<CustomerHelperItem>())
                RefreshItem(item, value => value.UpdateRequiredStoreLevel(true));
            foreach (var item in UnityEngine.Object.FindObjectsOfType<IceCreamHelperItem>())
                RefreshItem(item, value => value.UpdateRequiredStoreLevel(true));

            var bakerSetupMethod = AccessTools.Method(typeof(BakerItem), "Setup");
            foreach (var item in UnityEngine.Object.FindObjectsOfType<BakerItem>())
                RefreshItem(item, value => bakerSetupMethod?.Invoke(value, null));
        }

        private static void RefreshItem<T>(T item, Action<T> refresh) where T : UnityEngine.Object
        {
            if (item == null) return;

            try
            {
                refresh(item);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning($"Failed to refresh hiring UI for {typeof(T).Name}: {ex.Message}");
            }
        }


        public static void ApplyStaffUi(bool hasItem, bool isHired, UnityEngine.UI.Button hireButton, TMPro.TMP_Text reqLevelText, LocalizeStringEvent reqLevelLoc, Color incColor, Color compColor, GameObject obj1 = null, GameObject obj2 = null, TMPro.TMP_Text objLevelText = null, LocalizeStringEvent objLevelLoc = null)
        {
            if (hireButton == null) return;

            if (obj1 != null) obj1.SetActive(false);
            if (obj2 != null) obj2.SetActive(false);
            if (reqLevelLoc != null) reqLevelLoc.enabled = false;
            if (objLevelLoc != null) objLevelLoc.enabled = false;

            TMPro.TMP_Text btnText = hireButton.GetComponentInChildren<TMPro.TMP_Text>();
            LocalizeStringEvent btnLoc = btnText != null ? (btnText.GetComponent<LocalizeStringEvent>() ?? hireButton.GetComponent<LocalizeStringEvent>()) : null;

            if (btnLoc != null) btnLoc.enabled = false;

            if (isHired) return;

            if (objLevelText != null)
            {
                objLevelText.gameObject.SetActive(true); objLevelText.text = "You need a Storage to hire"; objLevelText.color = incColor;
            }

            if (!hasItem)
            {
                hireButton.interactable = false;
                if (btnText != null) btnText.text = "Locked";
                if (reqLevelText != null) { reqLevelText.gameObject.SetActive(true); reqLevelText.text = "Item required"; reqLevelText.color = incColor; }
            }
            else
            {
                hireButton.interactable = true;
                if (btnText != null) btnText.text = "Hire";
                if (reqLevelText != null) { reqLevelText.gameObject.SetActive(true); reqLevelText.text = "Item unlocked"; reqLevelText.color = compColor; }
            }
        }
    }

    [HarmonyPatch(typeof(HiringTab), "OnEnable")]
    public class HiringTabOnEnablePatch
    {
        [HarmonyPostfix]
        public static void Postfix() => PersonalUiManager.RefreshPersonalUI();
    }

    // ==========================================
    // STAFF ITEM PATCHES
    // ==========================================

    [HarmonyPatch(typeof(CashierItem))]
    public class CashierPatches
    {
        private static void ApplyArchipelagoRequirements(CashierItem item)
        {
            if (item == null) return;

            bool hasItem = item.CashierId <= ArchipelagoClient.GetReceivedCashierCount();
            item.m_HireLocked = !hasItem;
            if (!hasItem) return;
            if (item.m_CashierSetup == null) return;

            item.m_CashierSetup.RequiredStoreLevel = 0;
            item.m_CashierSetup.CheckoutGoalToUnlock = 0;
        }

        [HarmonyPatch(nameof(CashierItem.CanHire))]
        [HarmonyPrefix]
        public static void CanHirePrefix(CashierItem __instance) => ApplyArchipelagoRequirements(__instance);

        [HarmonyPatch(nameof(CashierItem.CanHire))]
        [HarmonyPostfix]
        public static void CanHirePostfix(CashierItem __instance, ref bool __result)
        {
            bool hasItem = __instance.CashierId <= ArchipelagoClient.GetReceivedCashierCount();
            if (!hasItem || __instance.Hired)
            {
                __result = false;
            }
        }

        [HarmonyPatch("UpdateRequiredStoreLevel", new Type[] { typeof(bool) })]
        [HarmonyPrefix]
        public static void UiPrefix(CashierItem __instance) => ApplyArchipelagoRequirements(__instance);

        [HarmonyPatch("UpdateRequiredStoreLevel", new Type[] { typeof(bool) })]
        [HarmonyPostfix]
        public static void UiPostfix(CashierItem __instance)
        {
            bool hasItem = __instance.CashierId <= ArchipelagoClient.GetReceivedCashierCount();
            __instance.m_HireLocked = !hasItem;

            PersonalUiManager.ApplyStaffUi(hasItem, __instance.Hired, __instance.m_HireButton, __instance.m_RequiredStoreLevelText, __instance.m_LocalizedRequiredStoreLevelText, __instance.m_IncompletedRequirementColor, __instance.m_CompletedRequirementColor, __instance.m_CheckoutObjectiveText != null ? __instance.m_CheckoutObjectiveText.gameObject : null);
        }
    }

    [HarmonyPatch(typeof(JanitorItem))]
    public class JanitorPatches
    {
        private static void ApplyArchipelagoRequirements(JanitorItem item)
        {
            if (item == null) return;

            bool hasItem = item.JanitorId <= ArchipelagoClient.GetReceivedJanitorCount();
            item.m_HireLocked = !hasItem;
            if (!hasItem) return;
            if (item.m_JanitorSetup == null) return;

            item.m_JanitorSetup.RequiredStoreLevel = 0;
        }

        [HarmonyPatch("CanHire")]
        [HarmonyPrefix]
        public static void CanHirePrefix(JanitorItem __instance) => ApplyArchipelagoRequirements(__instance);

        [HarmonyPatch("CanHire")]
        [HarmonyPostfix]
        public static void CanHirePostfix(JanitorItem __instance, ref bool __result)
        {
            bool hasItem = __instance.JanitorId <= ArchipelagoClient.GetReceivedJanitorCount();
            if (!hasItem || __instance.Hired)
            {
                __result = false;
            }
        }

        [HarmonyPatch("UpdateRequiredStoreLevel", new Type[] { typeof(bool) })]
        [HarmonyPrefix]
        public static void UiPrefix(JanitorItem __instance) => ApplyArchipelagoRequirements(__instance);

        [HarmonyPatch("UpdateRequiredStoreLevel", new Type[] { typeof(bool) })]
        [HarmonyPostfix]
        public static void UiPostfix(JanitorItem __instance)
        {
            bool hasItem = __instance.JanitorId <= ArchipelagoClient.GetReceivedJanitorCount();
            __instance.m_HireLocked = !hasItem;

            PersonalUiManager.ApplyStaffUi(hasItem, __instance.Hired, __instance.m_HireButton, __instance.m_RequiredStoreLevelText, __instance.m_LocalizedRequiredStoreLevelText, __instance.m_IncompletedRequirementColor, __instance.m_CompletedRequirementColor);
        }
    }

    [HarmonyPatch(typeof(RestockerItem))]
    public class RestockerPatches
    {
        private static void ApplyArchipelagoRequirements(RestockerItem item)
        {
            if (item == null) return;

            bool hasItem = item.RestockerId <= ArchipelagoClient.GetReceivedRestockerCount();
            item.m_HireLocked = !hasItem;
            if (!hasItem) return;
            if (item.m_RestockerSetup == null) return;

            item.m_RestockerSetup.RequiredStoreLevel = 0;
            item.m_RestockerSetup.RackGoalToUnlock = 0;
        }

        [HarmonyPatch("CanHire")]
        [HarmonyPrefix]
        public static void CanHirePrefix(RestockerItem __instance) => ApplyArchipelagoRequirements(__instance);

        [HarmonyPatch("CanHire")]
        [HarmonyPostfix]
        public static void CanHirePostfix(RestockerItem __instance, ref bool __result)
        {
            bool hasItem = __instance.RestockerId <= ArchipelagoClient.GetReceivedRestockerCount();
            if (!hasItem || __instance.Hired)
            {
                __result = false;
            }
        }

        [HarmonyPatch("UpdateRequiredStoreLevel", new Type[] { typeof(bool) })]
        [HarmonyPrefix]
        public static void UiPrefix(RestockerItem __instance) => ApplyArchipelagoRequirements(__instance);

        [HarmonyPatch("UpdateRequiredStoreLevel", new Type[] { typeof(bool) })]
        [HarmonyPostfix]
        public static void UiPostfix(RestockerItem __instance)
        {
            bool hasItem = __instance.RestockerId <= ArchipelagoClient.GetReceivedRestockerCount();
            __instance.m_HireLocked = !hasItem;

            PersonalUiManager.ApplyStaffUi(hasItem, __instance.Hired, __instance.m_HireButton, __instance.m_RequiredStoreLevelText, __instance.m_LocalizedRequiredStoreLevelText, __instance.m_IncompletedRequirementColor, __instance.m_CompletedRequirementColor, __instance.m_RackObjectiveText != null ? __instance.m_RackObjectiveText.gameObject : null);
        }
    }

    [HarmonyPatch(typeof(SecurityGuardItem))]
    public class SecurityPatches
    {
        private static void ApplyArchipelagoRequirements(SecurityGuardItem item)
        {
            if (item == null) return;
            bool hasItem = item.HelperId <= ArchipelagoClient.GetReceivedSecurityCount();
            item.m_HireLocked = !hasItem;
            if (!hasItem) return;
            if (item.m_SecurityGuardSetup == null) return;
            item.m_SecurityGuardSetup.RequiredStoreLevel = 0;
        }

        [HarmonyPatch("CanHire")]
        [HarmonyPrefix]
        public static void CanHirePrefix(SecurityGuardItem __instance) => ApplyArchipelagoRequirements(__instance);

        [HarmonyPatch("CanHire")]
        [HarmonyPostfix]
        public static void CanHirePostfix(SecurityGuardItem __instance, ref bool __result)
        {
            bool hasItem = __instance.HelperId <= ArchipelagoClient.GetReceivedSecurityCount();
            if (!hasItem || __instance.Hired)
            {
                __result = false;
            }
        }

        [HarmonyPatch("UpdateRequiredStoreLevel", new Type[] { typeof(bool) })]
        [HarmonyPrefix]
        public static void UiPrefix(SecurityGuardItem __instance) => ApplyArchipelagoRequirements(__instance);

        [HarmonyPatch("UpdateRequiredStoreLevel", new Type[] { typeof(bool) })]
        [HarmonyPostfix]
        public static void UiPostfix(SecurityGuardItem __instance)
        {
            bool hasItem = __instance.HelperId <= ArchipelagoClient.GetReceivedSecurityCount();
            __instance.m_HireLocked = !hasItem;

            PersonalUiManager.ApplyStaffUi(hasItem, __instance.Hired, __instance.m_HireButton, __instance.m_RequiredStoreLevelText, __instance.m_LocalizedRequiredStoreLevelText, __instance.m_IncompletedRequirementColor, __instance.m_CompletedRequirementColor);
        }
    }

    [HarmonyPatch(typeof(CustomerHelperItem))]
    public class CustomerHelperPatches
    {
        private static void ApplyArchipelagoRequirements(CustomerHelperItem item)
        {
            if (item == null) return;
            bool hasItem = item.HelperId <= ArchipelagoClient.GetReceivedHelperCount();
            item.m_HireLocked = !hasItem;
            if (!hasItem) return;
            if (item.m_CustomerHelperSetup == null) return;
            item.m_CustomerHelperSetup.RequiredStoreLevel = 0;
            item.m_CustomerHelperSetup.SelfCheckoutCountToUnlock = 0;
        }

        [HarmonyPatch("CanHire")]
        [HarmonyPrefix]
        public static void CanHirePrefix(CustomerHelperItem __instance) => ApplyArchipelagoRequirements(__instance);

        [HarmonyPatch("CanHire")]
        [HarmonyPostfix]
        public static void CanHirePostfix(CustomerHelperItem __instance, ref bool __result)
        {
            bool hasItem = __instance.HelperId <= ArchipelagoClient.GetReceivedHelperCount();
            if (!hasItem || __instance.Hired)
            {
                __result = false;
            }
        }

        [HarmonyPatch("UpdateRequiredStoreLevel", new Type[] { typeof(bool) })]
        [HarmonyPrefix]
        public static void UiPrefix(CustomerHelperItem __instance) => ApplyArchipelagoRequirements(__instance);

        [HarmonyPatch("UpdateRequiredStoreLevel", new Type[] { typeof(bool) })]
        [HarmonyPostfix]
        public static void UiPostfix(CustomerHelperItem __instance)
        {
            bool hasItem = __instance.HelperId <= ArchipelagoClient.GetReceivedHelperCount();
            __instance.m_HireLocked = !hasItem;

            PersonalUiManager.ApplyStaffUi(hasItem, __instance.Hired, __instance.m_HireButton, __instance.m_RequiredStoreLevelText, __instance.m_LocalizedRequiredStoreLevelText, __instance.m_IncompletedRequirementColor, __instance.m_CompletedRequirementColor, __instance.m_CheckoutObjectiveText != null ? __instance.m_CheckoutObjectiveText.gameObject : null);
        }
    }

    [HarmonyPatch(typeof(IceCreamHelperItem))]
    public class IceCreamHelperPatches
    {
        private static void ApplyArchipelagoRequirements(IceCreamHelperItem item)
        {
            if (item == null) return;
            bool hasItem = item.ID <= ArchipelagoClient.GetReceivedIceCreamHelperCount();
            item.m_HireLocked = !hasItem;
            if (!hasItem) return;
            if (item.m_IceCreamHelperSetup == null) return;
            item.m_IceCreamHelperSetup.RequiredStoreLevel = 0;
        }

        [HarmonyPatch("CanHire")]
        [HarmonyPrefix]
        public static void CanHirePrefix(IceCreamHelperItem __instance) => ApplyArchipelagoRequirements(__instance);

        [HarmonyPatch("CanHire")]
        [HarmonyPostfix]
        public static void CanHirePostfix(IceCreamHelperItem __instance, ref bool __result)
        {
            bool hasItem = __instance.ID <= ArchipelagoClient.GetReceivedIceCreamHelperCount();
            if (!hasItem || __instance.Hired)
            {
                __result = false;
            }
        }

        [HarmonyPatch("UpdateRequiredStoreLevel", new Type[] { typeof(bool) })]
        [HarmonyPrefix]
        public static void UiPrefix(IceCreamHelperItem __instance) => ApplyArchipelagoRequirements(__instance);

        [HarmonyPatch("UpdateRequiredStoreLevel", new Type[] { typeof(bool) })]
        [HarmonyPostfix]
        public static void UiPostfix(IceCreamHelperItem __instance)
        {
            bool hasItem = __instance.ID <= ArchipelagoClient.GetReceivedIceCreamHelperCount();
            __instance.m_HireLocked = !hasItem;

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
