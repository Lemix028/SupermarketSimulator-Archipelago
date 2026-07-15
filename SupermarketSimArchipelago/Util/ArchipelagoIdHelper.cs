using Gley.TrafficSystem;
using System;
using System.Collections.Generic;

namespace SupermarketArchipelago
{
    public enum ApItemType
    {
        License,
        Section,
        Cashier,
        Janitor,
        Restocker,
        Security,
        CHelper,
        Vehicle,
        Furniture,
        StorageUpgrade, 
        LoanAuth,    
        Filler,
        Trap,
        Unknown
    }

    public enum ApLocationType
    {
        StoreLevel,
        DayCompleted,
        StorageUpgrade,
        MoneyMilestone,
        Unknown
    }

    public static class ArchipelagoIdHelper
    {
        // Item Offsets
        public const int LicenseBaseID = 100;
        public const int SectionBaseID = 200;
        public const int CashierBaseID = 300;
        public const int JanitorBaseID = 310;
        public const int RestockerBaseID = 320;
        public const int SecurityBaseID = 330;
        public const int CHelperBaseID = 340;
        public const int VehicleBaseID = 350;
        public const int FurnitureBaseID = 400;
        public const int FillerBaseID = 500;
        public const int TrapBaseID = 700;
        public const int StorageUpgradeBaseID = 800; 
        public const int LoanAuthBaseID = 850;       

        // Location Offsets
        public const int StoreLevelBaseID = 200000;
        public const int DaysCompletedBaseID = 210000;
        public const int StorageUpgradeLocationBaseID = 220000;
        public const int MoneyMilestoneBaseID = 230000;

        private static readonly Dictionary<int, int> ApToGameFurniture = new Dictionary<int, int>
        {
            { 401, 1 },   // Normal Shelf (Shelf)
            { 402, 23 },  // Single Shelf (Shelf Single)
            { 403, 37 },  // Half Shelf (Shelf Half)
            { 404, 51 },  // Shelf Inner Corner
            { 405, 65 },  // Shelf Outer Corner
            { 406, 79 },  // Shelf Quad
            { 407, 93 },  // Shelf Quad Half
            { 408, 2 },   // Fridge Single
            { 409, 3 },   // Double Fridge
            { 410, 148 }, // Fridge Mini
            { 411, 162 }, // Display Fridge Single
            { 412, 176 }, // Display Fridge Double
            { 413, 107 }, // Single Freezer (Freezer Single)
            { 414, 4 },   // Freezer (Freezer)
            { 415, 134 }, // Triple Freezer (Freezer Triple)
            { 416, 6 },   // Checkout Counter
            { 417, 8 },   // Checkout Counter Mirrored
            { 418, 7 },   // Small Rack
            { 419, 9 },   // Tall Rack
            { 420, 5 },   // Spot Light
            { 421, 216 }, // Self Checkout Counter
            { 422, 217 }, // Self Checkout Counter Mirrored
            { 423, 218 }, // Speaker
            { 424, 219 }, // Category Sign
            { 425, 220 }, // Security Camera
            { 426, 221 }, // Security Antenna
            { 427, 223 }, // Scale
            { 428, 222 }, // Produce Stall
            { 429, 312 }  // Trash Can
        };

        // Automatic reverse-lookup table (Game ID -> AP ID)
        private static readonly Dictionary<int, int> GameToApFurniture = new Dictionary<int, int>();

        /// <summary>
        /// Static Constructor to automatically build the reverse lookup list
        /// </summary>
        static ArchipelagoIdHelper()
        {
            foreach (var kvp in ApToGameFurniture)
            {
                GameToApFurniture[kvp.Value] = kvp.Key;
            }
        }

        /// <summary>
        /// Translates a clean sequential AP ID (401-429) to the game's actual internal asset ID
        /// </summary>
        public static int FurnitureApToGame(int apID)
        {
            if (ApToGameFurniture.TryGetValue(apID, out int gameID))
            {
                return gameID;
            }
            return apID - FurnitureBaseID; // Safe fallback
        }

        /// <summary>
        /// Translates the game's actual internal asset ID back to our clean sequential AP ID
        /// </summary>
        public static int FurnitureGameToAp(int gameID)
        {
            if (GameToApFurniture.TryGetValue(gameID, out int apID))
            {
                return apID;
            }
            return FurnitureBaseID + gameID; // Safe fallback
        }

        /// <summary>
        /// Determines the item category based on the Archipelago network ID range.
        /// </summary>
        public static ApItemType GetItemType(long apID)
        {
            if (apID >= LicenseBaseID && apID < 200) return ApItemType.License;
            if (apID >= SectionBaseID && apID < 300) return ApItemType.Section;
            if (apID >= CashierBaseID && apID < 310) return ApItemType.Cashier;
            if (apID >= JanitorBaseID && apID < 320) return ApItemType.Janitor;
            if (apID >= RestockerBaseID && apID < 330) return ApItemType.Restocker;
            if (apID >= SecurityBaseID && apID < 340) return ApItemType.Security;
            if (apID >= CHelperBaseID && apID < 350) return ApItemType.CHelper;
            if (apID >= VehicleBaseID && apID < 400) return ApItemType.Vehicle;
            if (apID >= FurnitureBaseID && apID < 500) return ApItemType.Furniture; 
            if (apID >= FillerBaseID && apID < 700) return ApItemType.Filler;
            if (apID >= TrapBaseID && apID < 800) return ApItemType.Trap;
            if (apID >= StorageUpgradeBaseID && apID < 850) return ApItemType.StorageUpgrade; 
            if (apID >= LoanAuthBaseID && apID < 900) return ApItemType.LoanAuth;
            
            return ApItemType.Unknown;
        }

        /// <summary>
        /// Subtracts the Archipelago base offset to get the clean internal game index.
        /// </summary>
        public static int ToGameID(long apID)
        {
            if (apID >= LicenseBaseID && apID < 200) return (int)(apID - LicenseBaseID);
            if (apID >= SectionBaseID && apID < 300) return (int)(apID - SectionBaseID);
            if (apID >= CashierBaseID && apID < 310) return (int)(apID - CashierBaseID);
            if (apID >= JanitorBaseID && apID < 320) return (int)(apID - JanitorBaseID);
            if (apID >= RestockerBaseID && apID < 330) return (int)(apID - RestockerBaseID);
            if (apID >= SecurityBaseID && apID < 340) return (int)(apID - SecurityBaseID);
            if (apID >= CHelperBaseID && apID < 350) return (int)(apID - CHelperBaseID);
            if (apID >= VehicleBaseID && apID < 400) return (int)(apID - VehicleBaseID);

            // --- UPDATED FURNITURE CONVERSION ---
            if (apID >= FurnitureBaseID && apID < 500) return FurnitureApToGame((int)apID);

            if (apID >= FillerBaseID && apID < 700) return (int)(apID - FillerBaseID);
            if (apID >= TrapBaseID && apID < 800) return (int)(apID - TrapBaseID);
            if (apID >= StorageUpgradeBaseID && apID < 850) return (int)(apID - StorageUpgradeBaseID); 
            if (apID >= LoanAuthBaseID && apID < 900) return (int)(apID - LoanAuthBaseID);
            return (int)apID;
        }

        /// <summary>
        /// Maps an incoming network location check to its respective category group.
        /// </summary>
        public static ApLocationType GetLocationType(long locationID)
        {
            if (locationID > StoreLevelBaseID && locationID <= StoreLevelBaseID + 200) return ApLocationType.StoreLevel;
            if (locationID > DaysCompletedBaseID && locationID <= DaysCompletedBaseID + 1000) return ApLocationType.DayCompleted;
            if (locationID > StorageUpgradeLocationBaseID && locationID <= StorageUpgradeLocationBaseID + 20) return ApLocationType.StorageUpgrade;
            if (locationID > MoneyMilestoneBaseID && locationID <= MoneyMilestoneBaseID + 500) return ApLocationType.MoneyMilestone;
            return ApLocationType.Unknown;
        }

        public static int FromStoreLevel(int level) => StoreLevelBaseID + level;
        public static int FromDayCompleted(int day) => DaysCompletedBaseID + day;
        public static int FromStorageUpgrade(int upgrade) => StorageUpgradeLocationBaseID + upgrade;
        public static int FromMoneyMilestone(int money) => MoneyMilestoneBaseID + (money / 1000);
    }
}