using SupermarketArchipelago;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupermarketArchipelago
{
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
