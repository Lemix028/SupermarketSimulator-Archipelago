using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using BepInEx.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SupermarketArchipelago
{
    public static class ArchipelagoClient
    {
        private static ArchipelagoSession _session;
        private static ManualLogSource _log;
        public static bool IsConnected => _session != null && _session.Socket.Connected;

        // Fast lookup collection for all currently unlocked item IDs
        private static List<ItemInfo> ReceivedItemIDs = new List<ItemInfo>();
        private static List<long> SentLocationIDs = new List<long>();
        public static Dictionary<long, ScoutedItemInfo> ScoutedLocationsCache = new();

        // Receiving Items
        private static ConcurrentQueue<ItemInfo> _pendingItems = new ConcurrentQueue<ItemInfo>();
        public static bool IsStoreReady = false;

        public static void Initialize(ManualLogSource logger)
        {
            _log = logger;
            _log.LogInfo("Module ready.");
        }

        public static async void Connect(string serverUrl, string slotName, string password = null)
        {
            if (IsConnected) return;

            try
            {
                _log.LogInfo($"Attempting to connect to {serverUrl} as slot '{slotName}'...");

                // Create session using standard .NET sockets
                _session = ArchipelagoSessionFactory.CreateSession(serverUrl);

                // Attach item receiver event handler
                _session.Items.ItemReceived += OnItemReceived;


                // Try to login to the server
                var result = _session.TryConnectAndLogin(
                    "Supermarket Simulator", // Must match the AP world name exactly
                    slotName,
                    ItemsHandlingFlags.AllItems, // We want to know about all items (own and remote)
                    new Version(0, 6, 4),        // Min AP version required
                    password: password
                );


                // Check connection result and cast successful login to access slot data
                if (result is LoginSuccessful loginSuccess)
                {
                    _log.LogInfo("Connection successful! Syncing items...");

                    string currentSeed = _session.RoomState.Seed;
                    ArchipelagoHistoryManager.Init(slotName, currentSeed);

                    SentLocationIDs = _session.Locations.AllLocationsChecked.ToList();

                    var missingLocations = _session.Locations.AllMissingLocations;
                    if (missingLocations.Count > 0)
                    {
                        var scouted = await _session.Locations.ScoutLocationsAsync(missingLocations.ToArray());
                        ScoutedLocationsCache = scouted ?? new Dictionary<long, ScoutedItemInfo>();
                    }

                    

                    // Retrieve and parse YAML options configured on the server
                    ArchipelagoConfig.ParseSlotData(loginSuccess.SlotData);
                    
                }
                else if (result is LoginFailure failure)
                {
                    _log.LogError($"Login failed: {string.Join(", ", failure.Errors)}");
                    Disconnect();
                }
                UnityMainThreadDispatcher.Enqueue(() => {
                    MainMenuPatch.RefreshButtonText();
                });
            }
            catch (Exception ex)
            {
                _log.LogError($"Critical connection error: {ex.Message}");
                Disconnect();
            }
        }

        public static void Disconnect()
        {
            if (_session != null)
            {
                
                _session.Items.ItemReceived -= OnItemReceived;
                _session = null;
            }
            ReceivedItemIDs.Clear();
            _session.Socket.DisconnectAsync().Wait();
            UnityMainThreadDispatcher.Enqueue(() => {
                MainMenuPatch.RefreshButtonText();
            });
            _log?.LogWarning("Session disconnected and cleared.");
        }





        private static void OnItemReceived(ReceivedItemsHelper helper)
        {
            ArchipelagoHistoryManager.WaitForInit();
            ItemInfo item = helper.DequeueItem();
            ApItemType type = ArchipelagoIdHelper.GetItemType(item.ItemId);

            //License and Section items are processed immediately, while other types are queued until the store is ready
            if (type == ApItemType.Trap || type == ApItemType.Filler)
            {
                if (!ArchipelagoHistoryManager.IsNew(item))
                    return;

                if (!IsStoreReady)
                {
                    _pendingItems.Enqueue(item);
                    return;
                }

                ProcessReceivedItem(item, true);
            }
            else
            {
                ReceivedItemIDs.Add(item);
                ProcessReceivedItem(item);
                
            }
            
            string itemName = item.ItemName ?? "Unknown";
            string playerName = item.Player?.Name ?? "Unknown";

            if (IsStoreReady)
            {
                if (type == ApItemType.Trap)
                {
                    ArchipelagoNotificationManager.Instance.Show($"Received Trap: '{playerName}' sent you {itemName}!", 1);
                }
                else if (type == ApItemType.Filler)
                {
                    ArchipelagoNotificationManager.Instance.Show($"Received Filler: {itemName}!", 0);
                }
                else
                {
                    ArchipelagoNotificationManager.Instance.Show($"Received Item: {itemName} by {playerName}!", 3);
                }
            }
        }

        /// <summary>
        /// Centralized method to process received items and trigger appropriate game logic based on item type.
        /// </summary>
        public static void ProcessReceivedItem(ItemInfo item, bool setHistory = false)
        {
            //Plugin.Log.LogInfo($"NEW processing {item.ItemName} and {ArchipelagoHistoryManager.CreateSignature(item)}");

            UnityMainThreadDispatcher.Enqueue(() =>
            {
                int apItemID = (int)item.ItemId;
                ApItemType type = ArchipelagoIdHelper.GetItemType(apItemID);
                int gameID = ArchipelagoIdHelper.ToGameID(apItemID);

                switch (type)
                {
                    case ApItemType.License: UIHelper.RefreshLicenseUI(gameID); break;
                    case ApItemType.Section: UIHelper.RefreshSectionUI(gameID); break;
                    case ApItemType.Cashier:
                    case ApItemType.Janitor:
                    case ApItemType.Restocker:
                    case ApItemType.CHelper:
                    case ApItemType.Security: UIHelper.RefreshPersonalUI(); break;
                    case ApItemType.Vehicle: UIHelper.RefreshVehicleUI(gameID); break;
                    case ApItemType.Furniture: UIHelper.RefreshFurnitureUI(gameID); break;
                    case ApItemType.Filler: FillerHandler.ProcessFiller(apItemID); break;
                    case ApItemType.Trap: TrapHandler.ProcessTrap(apItemID); break;
                    case ApItemType.StorageUpgrade: UIHelper.RefreshStorageUI(); break;
                    case ApItemType.LoanAuth: UIHelper.RefreshLoanUI(); break;
                    case ApItemType.Unknown: _log.LogWarning($"Unknown item type: {apItemID}"); break;
                }

                if(setHistory)
                    ArchipelagoHistoryManager.MarkAsProcessed(item);

     

                
                    
            });
        }

        public static void SetStoreReady(bool ready)
        {
            IsStoreReady = ready;
            if (ready)
            {
                while (_pendingItems.TryDequeue(out var item))
                {
                    ProcessReceivedItem(item);
                }
            }
        }

        public static bool CheckLocationAlreadySent(long locationId)
        {
            return SentLocationIDs.Any(x => x == locationId);
        }

        public static bool CheckIncomingLicense(int gameID)
        {
            int apID = gameID + ArchipelagoIdHelper.LicenseBaseID;
            return ReceivedItemIDs.Any(x => x.ItemId == apID);
        }

        public static bool CheckIncomingSection(int gameID)
        {
            int apID = gameID + ArchipelagoIdHelper.SectionBaseID;
            return ReceivedItemIDs.Any(x => x.ItemId == apID);
        }

        public static bool CheckIncomingCashier(int gameID)
        {
            int apID = gameID + ArchipelagoIdHelper.CashierBaseID;
            return ReceivedItemIDs.Any(x => x.ItemId == apID);
        }
        public static bool CheckIncomingVehicle(int vehicleID)
        {
            if (!ArchipelagoConfig.EnableVehicleLocks)
            {
                return true;
            }


            int apID = ArchipelagoIdHelper.VehicleBaseID + vehicleID; 

            if (ArchipelagoConfig.UnlockedVehicles != null && ArchipelagoConfig.UnlockedVehicles.Contains(apID))
            {
                return true;
            }

            if (ReceivedItemIDs != null)
            {
                return ReceivedItemIDs.Any(x => x.ItemId == apID);
            }

            return false;
        }
        public static bool CheckIncomingFurniture(int gameID)
        {
            if (!ArchipelagoConfig.EnableFurnitureLocks)
            {
                return true;
            }
            int apID = ArchipelagoIdHelper.FurnitureGameToAp(gameID);

            if (ArchipelagoConfig.UnlockedFurniture != null && ArchipelagoConfig.UnlockedFurniture.Contains(apID))
            {
                return true;
            }

            if (ReceivedItemIDs != null)
            {
                return ReceivedItemIDs.Any(x => x.ItemId == apID);
            }

            return false;
        }


        public static bool HasLoanUnlock(int loanID)
        {
            if (!ArchipelagoConfig.EnableLoanLocks) return true;
            if (ReceivedItemIDs == null) return false;

            int apID = ArchipelagoIdHelper.LoanAuthBaseID + loanID;
            return ReceivedItemIDs.Any(x => x.ItemId == apID);
        }

        public static int GetReceivedSectionCount()
        {
            return ReceivedItemIDs.Count(x => ArchipelagoIdHelper.GetItemType(x.ItemId) == ApItemType.Section);
        }

        public static int GetReceivedCashierCount()
        {
            return ReceivedItemIDs.Count(x => ArchipelagoIdHelper.GetItemType(x.ItemId) == ApItemType.Cashier);
        }

        public static int GetReceivedJanitorCount()
        {
            return ReceivedItemIDs.Count(x => ArchipelagoIdHelper.GetItemType(x.ItemId) == ApItemType.Janitor);
        }

        public static int GetReceivedRestockerCount()
        {
            return ReceivedItemIDs.Count(x => ArchipelagoIdHelper.GetItemType(x.ItemId) == ApItemType.Restocker);
        }

        public static int GetReceivedSecurityCount()
        {
            return ReceivedItemIDs.Count(x => ArchipelagoIdHelper.GetItemType(x.ItemId) == ApItemType.Security);
        }

        public static int GetReceivedHelperCount()
        {
            return ReceivedItemIDs.Count(x => ArchipelagoIdHelper.GetItemType(x.ItemId) == ApItemType.CHelper);
        }
        public static int GetReceivedStorageCount()
        {
            if (ReceivedItemIDs == null) return 0;

            return ReceivedItemIDs.Count(x => x.ItemId >= 801 && x.ItemId <= 820);
        }

 
        public static async void SendLocation(long locationId)
        {
            if (_session == null)
            {
                Debug.LogWarning($"Outgoing check dropped. Not connected to server. Location ID: {locationId}");
                return;
            }

            try
            {

                _session.Locations.CompleteLocationChecks(locationId);
                SentLocationIDs.Add(locationId);

            }
            catch (Exception ex)
            {
                Debug.LogError($"Critical network error while sending Location {locationId}: {ex.Message}");
            }
            try
            {
                //Notifaction 
                var scoutedDict = ScoutedLocationsCache.First(x => x.Key == locationId);
                if (scoutedDict.Value != null)
                {

                    string targetItemName = scoutedDict.Value?.ItemName ?? "Unknown";
                    string targetPlayerName = scoutedDict.Value?.Player?.Name ?? "Unknown";

                    UnityMainThreadDispatcher.Enqueue(() =>
                    {
                        ArchipelagoNotificationManager.Instance.Show($"Sent '{targetItemName}' to {targetPlayerName}!", 2);
                    });
                }
            }
            catch (Exception ex) { }

           
        }

        /// <summary>
        /// Send Goal Completion to the Archipelago server. 
        /// </summary>
        public static void SendGoalCompletion()
        {
            if (_session == null || !_session.Socket.Connected)
            {
                Plugin.Log.LogWarning("Error while sending Goal to server: Not connected");
                return;
            }

            try
            {

                _session.SetGoalAchieved();

            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error at Goal {ex.Message}");
            }
        }
    }
}