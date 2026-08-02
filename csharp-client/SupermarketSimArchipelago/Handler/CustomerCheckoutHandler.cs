using SupermarketArchipelago;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupermarketArchipelago
{
    public static class ArchipelagoCheckoutHandler
    {
        private static readonly Random rng = new Random();

        public static void CheckCustomerCheckoutLocation()
        {
            if (!ArchipelagoClient.IsConnected) return;

            int maxLocations = ArchipelagoConfig.CustomerCheckoutLocations;
            if (maxLocations <= 0) return;

            // Find next unsent Customer Checkout location index
            int nextCount = 0;
            for (int c = 1; c <= maxLocations; c++)
            {
                int locId = ArchipelagoIdHelper.FromCustomerCheckout(c);
                if (!ArchipelagoClient.CheckLocationAlreadySent(locId))
                {
                    nextCount = c;
                    break;
                }
            }

            if (nextCount <= 0 || nextCount > maxLocations) return;

            int chance = ArchipelagoConfig.CustomerCheckoutChance;
            if (rng.Next(1, 101) <= chance)
            {
                int locationId = ArchipelagoIdHelper.FromCustomerCheckout(nextCount);
                ArchipelagoClient.SendLocation(locationId);
            }
        }
    }
}
