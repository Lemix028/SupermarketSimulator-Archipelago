using HarmonyLib;
using UnityEngine;

namespace SupermarketArchipelago
{
    [HarmonyPatch(typeof(MoneyManager), "MoneyTransition")]
    public class MoneyTransitionPatch
    {
        [HarmonyPrefix]
        public static void Prefix(ref float amount, MoneyManager.TransitionType type)
        {
            if (type == MoneyManager.TransitionType.CHECKOUT_INCOME)
            {
                amount *= ArchipelagoConfig.CheckoutIncomeMultiplier;
            }
            else if (type == MoneyManager.TransitionType.CUSTOMIZATION && ArchipelagoConfig.FreeCustomizables)
            {
                amount = 0f;
            }
        }

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

    [HarmonyPatch(typeof(SaveManager), "CreateLoadNewSave")]
    public class CreateLoadNewSavePatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (ArchipelagoClient.IsConnected && SaveManager.Instance != null && SaveManager.Instance.Progression != null)
            {
                SaveManager.Instance.Progression.Money = ArchipelagoConfig.StartingCash;
                Plugin.Log.LogInfo($"Set starting cash for new game to: {ArchipelagoConfig.StartingCash}");
            }
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