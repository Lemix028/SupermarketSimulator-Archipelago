using HarmonyLib;
using UnityEngine;

namespace SupermarketArchipelago
{
    [HarmonyPatch(typeof(MoneyManager), "MoneyTransition")]
    public class MoneyTransitionPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            ArchipelagoMoneyHandler.CheckMoneyMilestones();
        }
    }

    [HarmonyPatch(typeof(DayCycleManager), nameof(DayCycleManager.StartNextDay))]
    public class MoneyDayEndFallbackPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            ArchipelagoMoneyHandler.CheckMoneyMilestones();
        }
    }

    // ==========================================
    // ARCHIPELAGO MONEY MILESTONE HANDLER
    // ==========================================
    public static class ArchipelagoMoneyHandler
    {
        public static void CheckMoneyMilestones()
        {
            if (!ArchipelagoConfig.EnableMoneyMilestones) return;
            if (!ArchipelagoClient.IsConnected) return;
            if (MoneyManager.Instance == null) return;
           

            float currentMoney = MoneyManager.Instance.Money;

            int maxMoney = ArchipelagoConfig.MaxMoneyMilestone;
            int interval = ArchipelagoConfig.MoneyMilestoneInterval;

            for (int money = interval; money <= maxMoney; money += interval)
            {
                if (currentMoney >= money)
                {
                    long locationId = ArchipelagoIdHelper.FromMoneyMilestone(money);
                    if (!ArchipelagoClient.CheckLocationAlreadySent(locationId))
                        ArchipelagoClient.SendLocation(locationId);

                }
            }
        }
    }
}