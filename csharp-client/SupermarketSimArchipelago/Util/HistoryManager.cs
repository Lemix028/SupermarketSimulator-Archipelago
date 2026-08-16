using System.Collections.Generic;
using System.IO;
using System.Threading;
using Archipelago.MultiClient.Net.Models; 
using BepInEx;

namespace SupermarketArchipelago
{
    public static class ArchipelagoHistoryManager
    {
        private static readonly ManualResetEventSlim _initEvent = new ManualResetEventSlim(false);

        private static HashSet<string> _processedSignatures = new HashSet<string>();
        private static string _currentHistoryPath;
        

        public static void PrepareForSession()
        {
            _initEvent.Reset();
        }


        public static void Init(string slotName, string seed)
        {
            _initEvent.Reset();

            try
            {
                string safeName = string.IsNullOrEmpty(slotName) ? "Default" : slotName;
                string fileName = $"AP_History_{safeName}_{seed}.txt";

                Directory.CreateDirectory(Paths.CachePath);
                _currentHistoryPath = Path.Combine(Paths.CachePath, fileName);

                _processedSignatures.Clear();

                if (File.Exists(_currentHistoryPath))
                {
                    var lines = File.ReadAllLines(_currentHistoryPath);
                    foreach (var line in lines)
                    {
                        _processedSignatures.Add(line);
                    }
                }
            }
            finally
            {
                _initEvent.Set();
            }
        }

        public static void WaitForInit()
        {
            _initEvent.Wait();
        }

        public static string CreateSignature(ItemInfo item)
            => $"{item.ItemId}_{item.LocationId}_{item.Player}";

        public static bool IsNew(ItemInfo item)
            => !_processedSignatures.Contains(CreateSignature(item));

        public static void MarkAsProcessed(ItemInfo item)
        {
            string sig = CreateSignature(item);
            if (_processedSignatures.Add(sig))
            {
                File.AppendAllText(_currentHistoryPath, sig + System.Environment.NewLine);
            }
        }
    }
}
