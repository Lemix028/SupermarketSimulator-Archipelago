using HarmonyLib;
using System;

namespace SupermarketArchipelago
{

    [HarmonyPatch(typeof(StoreLevelManager), nameof(StoreLevelManager.CheckLevelChange))]
    public class StoreLevelProgressPatch
    {

        [HarmonyPostfix]
        public static void Postfix(StoreLevelManager __instance)
        {
            int currentLevel = __instance.CurrentLevel;

            for (int lvl = ArchipelagoConfig.StoreLevelInterval; lvl <= currentLevel; lvl += ArchipelagoConfig.StoreLevelInterval)
            {
                if (lvl > ArchipelagoConfig.MaxStoreLevel) break;

                int locationId = ArchipelagoIdHelper.FromStoreLevel(lvl);
                ArchipelagoClient.SendLocation(locationId);
            }
        }
    }


    [HarmonyPatch(typeof(DayCycleManager), nameof(DayCycleManager.FinishTheDay))]
    public class DayCompletedProgressPatch
    {
        public static void Postfix(DayCycleManager __instance)
        {
            int completedDay = __instance.CurrentDay - 1;

            if (completedDay < 1 || completedDay > ArchipelagoConfig.MaxDaysCompleted) return;

            if (completedDay % ArchipelagoConfig.DaysCompletedInterval == 0)
            {
                int locationId = ArchipelagoIdHelper.FromDayCompleted(completedDay);
                ArchipelagoClient.SendLocation(locationId);
            }
        }
    }
}