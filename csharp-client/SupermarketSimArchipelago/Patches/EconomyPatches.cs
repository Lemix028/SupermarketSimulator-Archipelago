using HarmonyLib;
using SupermarketSimArchipelago;
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
        public static void Postfix(MoneyManager.TransitionType type)
        {
            ArchipelagoMoneyHandler.CheckMoneyMilestones();

            if (type == MoneyManager.TransitionType.CHECKOUT_INCOME)
            {
                ArchipelagoCheckoutHandler.CheckCustomerCheckoutLocation();
            }
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

   


}