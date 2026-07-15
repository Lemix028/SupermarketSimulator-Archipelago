using HarmonyLib;
using SupermarketArchipelago;
using System;

namespace SupermarketArchipelago
{
    public static class GoalHandler
    {
        private static bool _goalReached = false;

        public static void CheckLevelGoal()
        {
            if (_goalReached) return;

            if (ArchipelagoConfig.GoalType.Equals("Level", StringComparison.OrdinalIgnoreCase))
            {
                if (StoreLevelManager.Instance != null && StoreLevelManager.Instance.CurrentLevel >= ArchipelagoConfig.GoalValue)
                {
                    TriggerVictory();
                }
            }
        }


        public static void CheckDaysGoal()
        {
            if (_goalReached) return;

            if (ArchipelagoConfig.GoalType.Equals("Days", StringComparison.OrdinalIgnoreCase))
            {
                if (DayCycleManager.Instance != null && DayCycleManager.Instance.CurrentDay >= ArchipelagoConfig.GoalValue)
                {
                    TriggerVictory();
                }
            }
        }

        public static void CheckCurrentProgress()
        {
            CheckLevelGoal();
            CheckDaysGoal();
        }

        private static void TriggerVictory()
        {
            _goalReached = true;
            ArchipelagoClient.SendGoalCompletion();
        }
    }

    // ==========================================
    // HARMONY PATCHES FOR GOAL TRACKING
    // ==========================================

    [HarmonyPatch(typeof(StoreLevelManager), nameof(StoreLevelManager.RefreshLevel))]
    public class GoalLevelPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            GoalHandler.CheckLevelGoal();
        }
    }

    [HarmonyPatch(typeof(DayCycleManager), nameof(DayCycleManager.StartNextDay))]
    public class GoalDaysPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            GoalHandler.CheckDaysGoal();
        }
    }
}