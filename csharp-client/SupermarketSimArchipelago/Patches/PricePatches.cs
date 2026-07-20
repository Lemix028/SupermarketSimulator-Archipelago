using __Project__.Scripts.Computer.Vending_Machine;
using __Project__.Scripts.WholeSale;
using HarmonyLib;
using SupermarketSimArchipelago;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace SupermarketArchipelago
{
    [HarmonyPatch]
    public class ArchipelagoPricePatches
    {
        // ==========================================
        // Local Market Product Price
        // ==========================================
        [HarmonyPatch(typeof(LocalMarketProductDatabase), nameof(LocalMarketProductDatabase.GetCost), new Type[] { typeof(ProductSO) })]
        [HarmonyPostfix]
        public static void GetCostPostfix(ProductSO productSo, ref float __result)
        {
            if (productSo == null) return;

            //Randomize already randomized product prices, but keep the 50% discount ratio for local products
            float randomizedLocalPrice = ArchipelagoPriceManager.GetLocalProductCost(productSo.ID, productSo.BoxPrice*0.5f);

            __result = randomizedLocalPrice;

        }

        // ==========================================
        // Product Box Price
        // Set final Cart Price for products in the store, which will be paid
        // ==========================================
        [HarmonyPatch(typeof(ProductSO), nameof(ProductSO.BoxPrice), MethodType.Getter)]
        [HarmonyPostfix]
        public static void ProductBoxPriceGetterPostfix(ProductSO __instance, ref float __result)
        {
            if (__instance == null) return;

            float originalPrice = __result;
            float randomizedPrice = ArchipelagoPriceManager.GetProductCost(__instance.ID, originalPrice);

            __result = randomizedPrice;
        }

        // ==========================================
        // Furniture Price 
        // Set final Cart Price for products in the store, which will be paid
        // ==========================================
        [HarmonyPatch(typeof(CartManager), nameof(CartManager.Start))]
        [HarmonyPostfix]
        public static void CartManagerStartPostfix()
        {
            try
            {
                var furnitureAssets = Resources.FindObjectsOfTypeAll<FurnitureSO>();
                if (furnitureAssets != null && furnitureAssets.Length > 0)
                {
                    foreach (var furniture in furnitureAssets)
                    {
                        if (furniture == null) continue;

                        //Skip Vending Machines to avoid double randomization
                        if (furniture is VendingMachineSO)
                            continue;
                            
                        
                        float originalCost = furniture.Cost;
                        float randomizedCost = ArchipelagoPriceManager.GetFurniturePrice(furniture.ID, originalCost);

                        furniture.Cost = randomizedCost;
                        //Plugin.Log.LogError($"[Archipelago Prices] Furniture ID {furniture.ID} | Original: {originalCost}$ | Randomized: {randomizedCost}$");
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[Archipelago Prices] Furniture memory mutation failed: {ex.Message}");
            }
        }

        // Furniture Sell Price Randomization Hook
        [HarmonyPatch(typeof(FurnitureBoxInteraction), nameof(FurnitureBoxInteraction.SellFurniture), new Type[] { })]
        [HarmonyPrefix]
        public static void FurnitureSellPrefix(FurnitureBoxInteraction __instance)
        {
            if (__instance == null || __instance.m_FurnitureBoxData == null) return;

            int furnitureId = __instance.m_FurnitureBoxData.FurnitureID;
            var furnitureSO = __instance.m_FurnitureBoxData.Furniture;

            if (furnitureSO != null)
            {
                float originalCost = furnitureSO.Cost;
                float randomizedCost = ArchipelagoPriceManager.GetFurniturePrice(furnitureId, originalCost);
                furnitureSO.Cost = randomizedCost;
            }
        }

        // ==========================================
        // License Price Randomization UI and paid Price
        // ==========================================
        [HarmonyPatch(typeof(LicenseItem), nameof(LicenseItem.Setup), new Type[] { typeof(int) })]
        [HarmonyPostfix]
        public static void LicenseItemSetupPostfix(LicenseItem __instance)
        {
            if (__instance == null) return;

            float originalCost = __instance.m_Cost;
            float randomizedCost = ArchipelagoPriceManager.GetLicensePrice(__instance.m_licenseID, originalCost);

            //Set logic price
            __instance.m_Cost = randomizedCost;

            //Set UI
            if (__instance.m_CostText != null)
            {
                __instance.m_CostText.text = "$" + randomizedCost.ToString("F2");
            }

        }

        // ==========================================
        // Vehicle Price Randomization UI and paid Price
        // ==========================================
        [HarmonyPatch(typeof(VehicleSaleItem), nameof(VehicleSaleItem.OnEnable), new Type[] { })]
        [HarmonyPostfix]
        public static void VehicleOnEnablePostfix(VehicleSaleItem __instance)
        {
            if (__instance == null) return;

            int vehicleLevel = __instance.m_VehicleLevel;
            float originalPrice = __instance.m_Price;
            float randomizedPrice = ArchipelagoPriceManager.GetVehiclePrice(vehicleLevel, originalPrice);

            //Set logic price
            __instance.m_Price = randomizedPrice;

            //Set UI
            if (__instance.m_PriceText != null)
            {
                __instance.m_PriceText.text = "$" + randomizedPrice.ToString("F2");
            }
        }

        // ==========================================
        // WholeSale Offer Price randomization UI and paid Price
        // ==========================================
        [HarmonyPatch(typeof(WholeSaleManager), nameof(WholeSaleManager.CalculateOfferPrice), new Type[] { typeof(bool) })]
        [HarmonyPostfix]
        public static void CalculateOfferPricePostfix(WholeSaleManager __instance, bool isBuyOffer)
        {
            if (__instance == null) return;

            int productId = __instance.m_RandomProductID;
            float originalInitial = __instance.m_InitialPrice;
            float originalFinal = __instance.m_FinalPrice;

            if (originalInitial <= 0f) return;

            float discountRatio = originalFinal / originalInitial;

            float archInitialPrice = ArchipelagoPriceManager.GetProductCost(productId, originalInitial);
            float archFinalPrice = (float)Math.Round(archInitialPrice * discountRatio, 2);

            __instance.m_InitialPrice = archInitialPrice;
            __instance.m_FinalPrice = archFinalPrice;

            if (__instance.BoxMarketPrice > 0f)
            {
                __instance.BoxMarketPrice = ArchipelagoPriceManager.GetProductCost(productId, __instance.BoxMarketPrice);
            }
        }

        // ==========================================
        // Storage and Growth Section Price Randomization
        // ==========================================

        [HarmonyPatch(typeof(CartManager), nameof(CartManager.Start))]
        [HarmonyPostfix]
        public static void PatchStorageAndGrowthAssets()
        {
            // Storage Assets
            var storageAssets = Resources.FindObjectsOfTypeAll<StorageSO>();
            foreach (var storage in storageAssets)
            {
                storage.Cost = ArchipelagoPriceManager.GetStoragePrice(storage.ID, storage.Cost);
            }

            // Section Assets
            var sectionAssets = Resources.FindObjectsOfTypeAll<SectionSO>();
            foreach (var section in sectionAssets)
            {
                section.Cost = ArchipelagoPriceManager.GetSectionPrice(section.ID, section.Cost);
            }

            Plugin.Log.LogInfo("[Archipelago Prices] Storage and Growth sections randomized in RAM.");
        }


        // UI Hook for Growth Sections
        [HarmonyPatch(typeof(GrowthSectionItem), nameof(GrowthSectionItem.Setup), new Type[] { typeof(int) })]
        [HarmonyPostfix]
        public static void GrowthSectionSetupPostfix(GrowthSectionItem __instance)
        {
            if (__instance == null || __instance.m_Section == null || __instance.m_CostText == null) return;

            float randomizedCost = __instance.m_Section.Cost;
            __instance.m_CostText.text = "$" + randomizedCost.ToString("F2");
        }

        // UI Hook for Storage Sections
        [HarmonyPatch(typeof(StorageSectionItem), nameof(StorageSectionItem.Setup), new Type[] { typeof(int) })]
        [HarmonyPostfix]
        public static void StorageSectionSetupPostfix(StorageSectionItem __instance)
        {
            if (__instance == null || __instance.m_Section == null || __instance.m_CostText == null) return;

            float randomizedCost = __instance.m_Section.Cost;
            __instance.m_CostText.text = "$" + randomizedCost.ToString("F2");
        }

    }
}