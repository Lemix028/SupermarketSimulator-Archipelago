using SupermarketArchipelago;
using System;
using System.Collections.Generic;

namespace SupermarketArchipelago
{
    public static class ArchipelagoPriceManager
    {
        // Global cache storing all randomized prices ("ItemKey" -> RandomizedPrice)
        private static readonly Dictionary<string, float> PriceRegistry = new Dictionary<string, float>();


        public static void ClearRegistry()
        {
            PriceRegistry.Clear();
        }

        // Standard string.GetHashCode() changes its output every time the game restarts!
        private static int GetStableHash(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;

            unchecked
            {
                int hash = 23;
                foreach (char c in text)
                {
                    hash = hash * 31 + c;
                }
                return hash;
            }
        }


        private static float GetRandomizedPrice(string itemKey, float originalPrice)
        {
            if (PriceRegistry.TryGetValue(itemKey, out float cachedPrice))
            {
                return cachedPrice;
            }

            int mode = ArchipelagoConfig.PriceRandomization; // 0 = Disabled, 1 = 20%, 2 = 50%
            string seed = ArchipelagoConfig.Seed;

            if (mode == 0 || string.IsNullOrEmpty(seed))
                return originalPrice;

            float maxOffset = (mode == 1) ? 0.20f : 0.50f;

            int itemSeed = GetStableHash(seed) ^ GetStableHash(itemKey); 
            Random rand = new Random(itemSeed);

            // Calculate the multiplier between (1 - maxOffset) and (1 + maxOffset)
            double doubleOffset = (rand.NextDouble() * (maxOffset * 2)) - maxOffset;
            float multiplier = 1f + (float)doubleOffset;

            // Calculate and round the final price
            float finalPrice = (float)Math.Round(originalPrice * multiplier, 2);

            // Prevent negative or zero prices
            if (finalPrice < 1)
                finalPrice = 1;

            // Save inside the global list
            PriceRegistry[itemKey] = finalPrice;

            return finalPrice;
        }

        //Unique price getters for different item types, using the item ID as part of the key
        public static float GetProductCost(int productId, float originalCost)
        {
            return GetRandomizedPrice($"ProductCost_{productId}", originalCost);
        }
        public static float GetLocalProductCost(int productId, float originalCost)
        {
            return GetRandomizedPrice($"LocalProductCost_{productId}", originalCost);
        }

        public static float GetProductMarketPrice(int productId, float originalMarketPrice)
        {
            return GetRandomizedPrice($"ProductMarket_{productId}", originalMarketPrice);
        }

        public static float GetLicensePrice(int licenseId, float originalPrice)
        {
            return GetRandomizedPrice($"License_{licenseId}", originalPrice);
        }

        public static float GetFurniturePrice(int furnitureId, float originalPrice)
        {
            return GetRandomizedPrice($"Furniture_{furnitureId}", originalPrice);
        }

        public static float GetVehiclePrice(int vehicleId, float originalPrice)
        {
            return GetRandomizedPrice($"Vehicle_{vehicleId}", originalPrice);
        }

        public static float GetVendingUpgradePrice(int currentSlots, float originalPrice)
        {
            return GetRandomizedPrice($"VendingUpgrade_{currentSlots}", originalPrice);
        }

        public static float GetSectionPrice(int id, float original)
        {
            return GetRandomizedPrice($"Section_{id}", original);
        }

        public static float GetStoragePrice(int id, float original)
        {
            return GetRandomizedPrice($"Storage_{id}", original);

        }
    }
}