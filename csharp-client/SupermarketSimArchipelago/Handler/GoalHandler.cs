using HarmonyLib;
using SupermarketArchipelago;
using System;

namespace SupermarketArchipelago
{
    public static class GoalHandler
    {
        public static bool GoalReached = false;

        /// <summary>
        /// Resets the goal state. Call this when disconnecting or starting a new session.
        /// </summary>
        public static void Reset()
        {
            GoalReached = false;
        }

        public static void CheckLevelGoal()
        {
            if (GoalReached) return;

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
            if (GoalReached) return;

            if (ArchipelagoConfig.GoalType.Equals("Days", StringComparison.OrdinalIgnoreCase))
            {
                if (DayCycleManager.Instance != null && DayCycleManager.Instance.CurrentDay >= ArchipelagoConfig.GoalValue)
                {
                    TriggerVictory();
                }
            }
        }

        public static void CheckLicensesGoal()
        {
            if (GoalReached) return;

            if (ArchipelagoConfig.GoalType.Equals("All Licenses", StringComparison.OrdinalIgnoreCase))
            {
                if (ArchipelagoClient.AreAllRequiredLicensesPurchased())
                {
                    TriggerVictory();
                }
            }
        }

        public static void CheckCurrentProgress()
        {
            CheckLevelGoal();
            CheckDaysGoal();
            CheckLicensesGoal();
        }

        private static void TriggerVictory()
        {
            GoalReached = true;
            ArchipelagoClient.SendGoalCompletion();
            
            if (ArchipelagoNotificationManager.Instance != null)
            {
                ArchipelagoNotificationManager.Instance.Show("Goal Completed! Releasing all items!", 4);
            }
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