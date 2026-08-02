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
using System.Threading.Tasks;
using UnityEngine;
using static Archipelago.MultiClient.Net.Helpers.ArchipelagoSocketHelperDelagates;

namespace SupermarketArchipelago
{
    public static class ArchipelagoClient
    {
        private static ArchipelagoSession _session;
        private static ManualLogSource _log;
        public static bool IsConnected => _session != null && _session.Socket.Connected;

        // Credentials stored for auto-reconnect
        private static string _lastServerUrl;
        private static string _lastSlotName;
        private static string _lastPassword;
        private static bool _isReconnecting = false;
        private const int MaxReconnectAttempts = 5;

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

            // Store credentials for auto-reconnect
            _lastServerUrl = serverUrl;
            _lastSlotName  = slotName;
            _lastPassword  = password;

            try
            {
                _log.LogInfo($"Attempting to connect to {serverUrl} as slot '{slotName}'...");

                // Create session using standard .NET sockets
                _session = ArchipelagoSessionFactory.CreateSession(serverUrl);

                // Attach item receiver and connection-lost handler
                _session.Items.ItemReceived += OnItemReceived;
                _session.Socket.SocketClosed += OnSocketClosed;

                // Try to login to the server
                var result = _session.TryConnectAndLogin(
                    "Supermarket Simulator", // Must match the AP world name exactly
                    slotName,
                    ItemsHandlingFlags.AllItems, // We want to know about all items (own and remote)
                    new Version(0, 6, 7),        // Min AP version required
                    password: password
                );


                // Check connection result and cast successful login to access slot data
                if (result is LoginSuccessful loginSuccess)
                {
                    _log.LogInfo("Connection successful! Syncing items...");
                    _isReconnecting = false;

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
                try
                {
                    _session.Items.ItemReceived -= OnItemReceived;
                    _session.Socket.SocketClosed -= OnSocketClosed;
                }
                catch (Exception ex)
                {
                    _log?.LogWarning($"Error removing socket event listeners: {ex.Message}");
                }

                try
                {
                    if (_session.Socket != null && IsConnected)
                    {
                        _session.Socket.DisconnectAsync().Wait();
                    }
                }
                catch (Exception ex)
                {
                    // Ignore exceptions during socket disconnection (e.g. if the socket was already disconnected/offline)
                    _log?.LogWarning($"Ignored socket disconnect exception: {ex.Message}");
                }

                ReceivedItemIDs.Clear();
                ArchipelagoPriceManager.ClearRegistry();
                GoalHandler.Reset();

                _session = null;
                _log?.LogWarning("Session disconnected and cleared.");
            }
            UnityMainThreadDispatcher.Enqueue(() => {
                MainMenuPatch.RefreshButtonText();
            });
        }

        /// <summary>
        /// Called when the socket connection is lost unexpectedly.
        /// Triggers automatic reconnect with exponential backoff.
        /// </summary>
        private static void OnSocketClosed(string reason)
        {
            _log.LogWarning($"Connection lost: {reason}. Scheduling reconnect...");
            if (!_isReconnecting)
                _ = ReconnectWithBackoff();
        }

        /// <summary>
        /// Attempts to reconnect to the server using exponential backoff (1s, 2s, 4s, 8s, 16s).
        /// </summary>
        private static async Task ReconnectWithBackoff()
        {
            _isReconnecting = true;
            _session = null;

            UnityMainThreadDispatcher.Enqueue(() => MainMenuPatch.RefreshButtonText());

            for (int attempt = 0; attempt < MaxReconnectAttempts; attempt++)
            {
                int delaySec = (int)Math.Pow(2, attempt); // 1, 2, 4, 8, 16 seconds
                _log.LogInfo($"Reconnect attempt {attempt + 1}/{MaxReconnectAttempts} in {delaySec}s...");
                await Task.Delay(delaySec * 1000);

                if (IsConnected) break;

                Connect(_lastServerUrl, _lastSlotName, _lastPassword);

                // Brief wait for the async connect to settle
                await Task.Delay(2000);
                if (IsConnected) break;
            }

            _isReconnecting = false;

            if (!IsConnected)
                _log.LogError("Reconnect failed after maximum attempts. Please reconnect manually.");
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
            bool isSelfFound = _session != null && item.Player != null && item.Player.Slot == _session.ConnectionInfo.Slot;

            if (IsStoreReady)
            {
                UnityMainThreadDispatcher.Enqueue(() =>
                {
                    if (GoalHandler.GoalReached)
                        return;

                    if (type == ApItemType.Trap)
                    {
                        if (isSelfFound)
                            ArchipelagoNotificationManager.Instance.Show($"Found your '{itemName}'!", 1);
                        else
                            ArchipelagoNotificationManager.Instance.Show($"Received Trap: '{playerName}' sent you {itemName}!", 1);
                    }
                    else if (type == ApItemType.Filler)
                    {
                        if (isSelfFound)
                            ArchipelagoNotificationManager.Instance.Show($"Found your '{itemName}'!", 0);
                        else
                            ArchipelagoNotificationManager.Instance.Show($"Received Filler: {itemName}!", 0);
                    }
                    else
                    {
                        if (isSelfFound)
                            ArchipelagoNotificationManager.Instance.Show($"Found your '{itemName}'!", 3);
                        else
                            ArchipelagoNotificationManager.Instance.Show($"Received Item: '{itemName}' from {playerName}!", 3);
                    }
                });
            }
        }

        /// <summary>
        /// Centralized method to process received items and trigger appropriate game logic based on item type.
        /// </summary>
        public static void ProcessReceivedItem(ItemInfo item, bool setHistory = false)
        {

            UnityMainThreadDispatcher.Enqueue(() =>
            {
                int apItemID = (int)item.ItemId;
                ApItemType type = ArchipelagoIdHelper.GetItemType(apItemID);
                int gameID = ArchipelagoIdHelper.ToGameID(apItemID);

                switch (type)
                {
                    case ApItemType.License: 
                        UIHelper.RefreshLicenseUI(gameID); 
                        GoalHandler.CheckLicensesGoal(); 
                        break;
                    case ApItemType.Section: UIHelper.RefreshSectionUI(gameID); break;
                    case ApItemType.Cashier:
                    case ApItemType.Janitor:
                    case ApItemType.Restocker:
                    case ApItemType.CHelper:
                    case ApItemType.Security:
                    case ApItemType.Baker:
                    case ApItemType.IceCreamHelper: UIHelper.RefreshPersonalUI(); break;
                    case ApItemType.Vehicle: UIHelper.RefreshVehicleUI(gameID); break;
                    case ApItemType.Furniture: UIHelper.RefreshFurnitureUI(gameID); break;
                    case ApItemType.Filler: FillerHandler.ProcessFiller(apItemID); break;
                    case ApItemType.Trap: TrapHandler.ProcessTrap(apItemID); break;
                    case ApItemType.StorageUpgrade: UIHelper.RefreshStorageUI(); break;
                    case ApItemType.LoanAuth: UIHelper.RefreshLoanUI(); break;
                    case ApItemType.VendingSlot: UIHelper.RefreshVendingUI(); break;
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

                GoalHandler.CheckCurrentProgress();
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

            if (ArchipelagoConfig.unlockedVehicles != null && ArchipelagoConfig.unlockedVehicles.Contains(apID))
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

            if (ArchipelagoConfig.unlockedFurniture != null && ArchipelagoConfig.unlockedFurniture.Contains(apID))
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

        public static int GetReceivedLicenseCount()
        {
            if (ReceivedItemIDs == null) return 0;
            var allUnlocked = new HashSet<int>(ArchipelagoConfig.unlockedLicenses);
            foreach (var item in ReceivedItemIDs)
            {
                if (ArchipelagoIdHelper.GetItemType(item.ItemId) == ApItemType.License)
                {
                    allUnlocked.Add((int)item.ItemId);
                }
            }
            return allUnlocked.Count;
        }

        public static bool AreAllRequiredLicensesPurchased()
        {
            if (SaveManager.Instance == null || SaveManager.Instance.Progression == null)
            {
                Plugin.Log.LogWarning("[Goal] SaveManager.Instance or Progression is null, cannot check license goal.");
                return false;
            }

            var purchased = SaveManager.Instance.Progression.UnlockedLicenses;
            if (purchased == null)
            {
                Plugin.Log.LogWarning("[Goal] Progression.UnlockedLicenses is null, cannot check license goal.");
                return false;
            }

            var requiredLicenses = ArchipelagoConfig.RequiredLicenses;
            if (requiredLicenses == null || requiredLicenses.Count == 0)
            {
                Plugin.Log.LogWarning("[Goal] RequiredLicenses list is empty or null, cannot check license goal.");
                return false;
            }


            // Check if all required licenses in the seed are purchased in-game
            foreach (var lic in requiredLicenses)
            {
                if (!purchased.Contains(lic))
                {
                    return false; // Found a license that is required but not purchased in-game!
                }
            }

            return true;
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

        public static int GetReceivedIceCreamHelperCount()
        {
            return ReceivedItemIDs.Count(x => ArchipelagoIdHelper.GetItemType(x.ItemId) == ApItemType.IceCreamHelper);
        }

        public static int GetReceivedBakerCount()
        {
            return ReceivedItemIDs.Count(x => ArchipelagoIdHelper.GetItemType(x.ItemId) == ApItemType.Baker);
        }
        public static int GetReceivedStorageCount()
        {
            if (ReceivedItemIDs == null) return 0;
            return ReceivedItemIDs.Count(x =>
                x.ItemId >= ArchipelagoIdHelper.StorageUpgradeBaseID + 1 &&
                x.ItemId <= ArchipelagoIdHelper.StorageUpgradeBaseID + 20);
        }

        public static int GetReceivedVendingSlots()
        {
            if (ReceivedItemIDs == null) return 0;
            return ReceivedItemIDs.Count(x => x.ItemId == ArchipelagoIdHelper.VendingMachineBaseID);
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
                if (SentLocationIDs.Contains(locationId))
                    return;
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
                    bool isSelf = _session != null && scoutedDict.Value.Player != null && scoutedDict.Value.Player.Slot == _session.ConnectionInfo.Slot;

                    if (!isSelf)
                    {
                        UnityMainThreadDispatcher.Enqueue(() =>
                        {
                            ArchipelagoNotificationManager.Instance.Show($"Sent '{targetItemName}' to {targetPlayerName}!", 2);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogWarning($"Could not send location notification for ID {locationId}: {ex.Message}");
            }

           
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