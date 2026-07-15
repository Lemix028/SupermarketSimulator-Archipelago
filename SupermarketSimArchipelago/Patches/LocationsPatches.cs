using HarmonyLib;
using System;

namespace SupermarketArchipelago
{

    [HarmonyPatch(typeof(StoreLevelManager), "set_CurrentLevel")]
    public class StoreLevelProgressPatch
    {
        public static void Postfix(StoreLevelManager __instance, int value)
        {
            int newLevel = value;

            if (newLevel > ArchipelagoConfig.MaxStoreLevel) return;

            if (newLevel % ArchipelagoConfig.StoreLevelInterval == 0)
            {
                int locationId = ArchipelagoIdHelper.FromStoreLevel(newLevel);
                ArchipelagoClient.SendLocation(locationId);
            }
        }
    }


    [HarmonyPatch(typeof(DayCycleManager), nameof(DayCycleManager.FinishTheDay))]
    public class DayCompletedProgressPatch
    {
        public static void Postfix(DayCycleManager __instance)
        {
            int completedDay = __instance.CurrentDay;

            if (completedDay > ArchipelagoConfig.MaxDaysCompleted) return;

            if (completedDay % ArchipelagoConfig.DaysCompletedInterval == 0)
            {
                int locationId = ArchipelagoIdHelper.FromDayCompleted(completedDay);
                ArchipelagoClient.SendLocation(locationId);
            }
        }
    }
}