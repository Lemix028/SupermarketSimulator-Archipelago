using __Project__.Scripts.Computer.Vending_Machine; 
using HarmonyLib;
using SupermarketSimArchipelago;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace SupermarketArchipelago
{
    [HarmonyPatch(typeof(VendingMachineTab))]
    public class VendingMachineTabPatches
    {
        [HarmonyPatch(nameof(VendingMachineTab.OnEnable))]
        [HarmonyPostfix]
        public static void OnEnablePostfix(VendingMachineTab __instance) => ApplyVendingLock(__instance);

        [HarmonyPatch(nameof(VendingMachineTab.ShowSlot))]
        [HarmonyPostfix]
        public static void ShowSlotPostfix(VendingMachineTab __instance) => ApplyVendingLock(__instance);

        [HarmonyPatch(nameof(VendingMachineTab.SelectedSlot))]
        [HarmonyPostfix]
        public static void SelectedSlotPostfix(VendingMachineTab __instance) => ApplyVendingLock(__instance);

        [HarmonyPatch("OnEnableControl")]
        [HarmonyPostfix]
        public static void OnEnableControlPostfix(VendingMachineTab __instance) => ApplyVendingLock(__instance);

        [HarmonyPatch("OnMoneyChange")]
        [HarmonyPostfix]
        public static void OnMoneyChangePostfix(VendingMachineTab __instance) => ApplyVendingLock(__instance);


        public static void ApplyVendingLock(VendingMachineTab __instance)
        {
            if (__instance == null) return;

            int currentSlotsCount = 0;
            if (__instance.slotParent != null)
            {
                var activeSlots = __instance.slotParent.GetComponentsInChildren<VendingMachineComputerSlot>(true);
                currentSlotsCount = (activeSlots != null) ? activeSlots.Length : 0;
            }

            int nextSlotRequirement = currentSlotsCount + 1;

            int receivedSlotsCount = ArchipelagoClient.GetReceivedVendingSlots();
            bool hasItem = nextSlotRequirement <= receivedSlotsCount;

            float originalPrice = (__instance.m_VendingMachineSO != null && __instance.m_VendingMachineSO.Cost > 0f)
                ? __instance.m_VendingMachineSO.Cost
                : 500f;

            //Set randomized price based on current slots count and original price
            float currentPrice = ArchipelagoPriceManager.GetVendingUpgradePrice(currentSlotsCount, originalPrice);

            if (__instance.m_VendingMachineSO != null)
            {
                __instance.m_VendingMachineSO.Cost = currentPrice;
            }

            Plugin.Log.LogInfo($"VendingMachineTabPatches: CurrentSlots={currentSlotsCount}, NextSlotRequirement={nextSlotRequirement}, ReceivedSlots={receivedSlotsCount}, HasItem={hasItem}, OriginalPrice={originalPrice}, CurrentPrice={currentPrice}");
            if (__instance.buyButton != null)
            {
                if (!hasItem)
                {
                    __instance.buyButton.interactable = false;

                    var localizers = __instance.buyButton.GetComponentsInChildren<LocalizeStringEvent>(true);
                    foreach (var loc in localizers)
                    {
                        if (loc != null) loc.enabled = false;
                    }

                    var tmps = __instance.buyButton.GetComponentsInChildren<TMP_Text>(true);
                    bool isFirst = true;
                    foreach (var tmp in tmps)
                    {
                        if (tmp != null)
                        {
                            tmp.enableAutoSizing = false;
                            tmp.fontSize = 12f;
                            if (isFirst)
                            {
                                
                                tmp.text = "Item required";
                                isFirst = false;
                                tmp.alignment = TextAlignmentOptions.Left;
                            }
                            else
                            {
                                tmp.text = ""; 
                            }
                        }
                    }

      
                }
                else
                {
                    __instance.buyButton.interactable = true;
                    var localizers = __instance.buyButton.GetComponentsInChildren<LocalizeStringEvent>(true);
                    foreach (var loc in localizers)
                    {
                        if (loc != null)
                        {
                            loc.enabled = false;
                        }
                    }

                    var tmps = __instance.buyButton.GetComponentsInChildren<TMP_Text>(true);
                    bool isFirst = true;
                    foreach (var tmp in tmps)
                    {
                        if (tmp != null)
                        {
                            tmp.enableAutoSizing = false;
                            tmp.fontSize = 12f;
                            if (isFirst)
                            {
                                tmp.text = $"Buy {currentPrice:F2}$";
                                tmp.alignment = TextAlignmentOptions.Left;
                                isFirst = false;
                            }
                            else
                            {
                                tmp.text = "";
                            }
                        }
                    }
                }
            }
        }
    }
}