using System;
using System.Collections;
using System.Collections.Generic;

namespace SupermarketArchipelago
{
    // ==========================================
    // ARCHIPELAGO CONFIGURATION STORAGE
    // ==========================================
    public static class ArchipelagoConfig
    {
        public static bool EnableFurnitureLocks { get; private set; } = true;
        public static bool EnableVehicleLocks { get; private set; } = true;
        public static bool EnableStorageLocks { get; private set; } = true; 
        public static bool EnableLoanLocks { get; private set; } = true;

        public static string GoalType { get; private set; } = "Level";
        public static int GoalValue { get; private set; } = 50;

        public static int MaxStoreLevel { get; private set; } = 50;
        public static int StoreLevelInterval { get; private set; } = 1;
        public static int MaxDaysCompleted { get; private set; } = 50;
        public static int DaysCompletedInterval { get; private set; } = 1;

        public static HashSet<int> UnlockedLicenses { get; private set; } = new HashSet<int>();
        public static HashSet<int> UnlockedVehicles { get; private set; } = new HashSet<int>();
        public static HashSet<int> UnlockedFurniture { get; private set; } = new HashSet<int>();

        public static bool EnableMoneyMilestones { get; private set; } = true;
        public static int MaxMoneyMilestone { get; private set; } = 25000;
        public static int MoneyMilestoneInterval { get; private set; } = 5000;

        /// <summary>
        /// Parses the slot data dictionary received from the Archipelago server.
        /// </summary>
        public static void ParseSlotData(Dictionary<string, object> slotData)
        {
            if (slotData == null)
            {
                Plugin.Log.LogError("Slot Data is null! Cannot initialize rules.");
                return;
            }

            try
            {
                EnableFurnitureLocks = GetSafeInt(slotData, "enable_furniture_locks", 1) == 1;
                EnableVehicleLocks = GetSafeInt(slotData, "enable_vehicle_locks", 1) == 1;
                EnableStorageLocks = GetSafeInt(slotData, "enable_storage_locks", 1) == 1; 
                EnableLoanLocks = GetSafeInt(slotData, "enable_loan_locks", 1) == 1;       

                MaxStoreLevel = GetSafeInt(slotData, "max_store_level", 50);
                StoreLevelInterval = GetSafeInt(slotData, "store_level_interval", 1);
                MaxDaysCompleted = GetSafeInt(slotData, "max_days_completed", 50);
                DaysCompletedInterval = GetSafeInt(slotData, "days_completed_interval", 1);

                EnableMoneyMilestones = GetSafeInt(slotData, "enable_money_milestones", 1) == 1;
                MaxMoneyMilestone = GetSafeInt(slotData, "max_money_milestone", 25000);
                MoneyMilestoneInterval = GetSafeInt(slotData, "money_milestone_interval", 5000);

                int goalIndex = GetSafeInt(slotData, "goal", 0);
                if (goalIndex == 1)
                {
                    GoalType = "Days";
                    GoalValue = MaxDaysCompleted;
                }
                else
                {
                    GoalType = "Level";
                    GoalValue = MaxStoreLevel;
                }

                
                UnlockedLicenses = GetSafeIntSet(slotData, "default_licenses");
                UnlockedVehicles = GetSafeIntSet(slotData, "default_vehicles");
                UnlockedFurniture = GetSafeIntSet(slotData, "default_furniture");

                Plugin.Log.LogInfo($"Applied Server Options!"); //+
                //                   $"Goal: {GoalType} {GoalValue} | " +
                //                   $"Level Interval: {StoreLevelInterval}, Days Interval: {DaysCompletedInterval} | " +
                //                   $"Furniture Locked: {EnableFurnitureLocks} (Defaults: {UnlockedFurniture.Count}) | " +
                //                   $"Vehicles Locked: {EnableVehicleLocks} (Defaults: {UnlockedVehicles.Count}) | " +
                //                   $"License Defaults: {UnlockedLicenses.Count}");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Critical exception caught during slot data processing: {ex}");
            }
        }

        private static int GetSafeInt(Dictionary<string, object> dict, string key, int defaultValue)
        {
            if (dict.TryGetValue(key, out var val))
            {
                try
                {
                    return Convert.ToInt32(val);
                }
                catch (Exception ex)
                {
                    Plugin.Log.LogError($"Cast conversion failed for key '{key}': {ex.Message}");
                }
            }
            return defaultValue;
        }

        private static HashSet<int> GetSafeIntSet(Dictionary<string, object> dict, string key)
        {
            var set = new HashSet<int>();
            if (!dict.TryGetValue(key, out var val) || val == null) return set;

            if (val is IEnumerable enumerable && !(val is string))
            {
                foreach (var element in enumerable)
                {
                    try
                    {
                        set.Add(Convert.ToInt32(element));
                    }
                    catch { /* ignore corrupted elements */ }
                }
            }
            else if (val is string str)
            {
                var parts = str.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    if (int.TryParse(part.Trim(), out int id))
                    {
                        set.Add(id);
                    }
                }
            }
            return set;
        }
    }
}