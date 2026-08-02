#nullable disable
using SupermarketArchipelago;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.Components;

namespace SupermarketArchipelago
{
    public static class UIHelper
    {
        /// <summary>
        /// Update License UI
        /// </summary>
        public static void RefreshLicenseUI(int licenseID)
        {
            var licenseItems = GameObject.FindObjectsOfType<LicenseItem>(true);
            if (licenseItems != null)
            {
                foreach (var item in licenseItems)
                {
                    if (item != null && item.m_licenseID == licenseID)
                    {
                        LicenseUiHelper.ForceArchipelagoVisuals(item);
                        item.OnLicenseActivatedExternally(licenseID, true);
                        item.CheckIfReachedRequiredLevel(true);
                        item.RefreshFromState();
                    }
                }
            }

            var tab = GameObject.FindObjectOfType<LicensesTab>(true);
            if (tab != null)
            {
                try
                {
                    tab.ApplyFiltersAndSort();
                }
                catch { }
            }
        }

        /// <summary>
        /// Update Section UI
        /// </summary>
        public static void RefreshSectionUI(int gameSectionID)
        {
            // Find all section items (including inactive ones when tab is closed)
            var items = GameObject.FindObjectsOfType<GrowthSectionItem>(true);
            if (items == null) return;

            foreach (var item in items)
            {
                // Trigger native checks to fire our Harmony UI postfixes
                item.CheckRequirements();
            }
        }

        /// <summary>
        /// Update Personal UI
        /// </summary>
        public static void RefreshPersonalUI()
        {
            PersonalUiManager.RefreshPersonalUI();
        }

        /// <summary>
        /// Update Furniture UI Catalog
        /// </summary>
        public static void RefreshFurnitureUI(int gameFurnitureID)
        {
            var items = GameObject.FindObjectsOfType<FurnitureSalesItem>(true);
            if (items == null) return;

            foreach (var item in items)
            {
                if (item != null && item.ProductId == gameFurnitureID)
                {
                    FurnitureUiHelper.ForceArchipelagoVisuals(item);
                    break;
                }
            }
        }

        /// <summary>
        /// Update Vehicle UI Catalog
        /// </summary>
        public static void RefreshVehicleUI(int gameVehicleID)
        {
            var items = GameObject.FindObjectsOfType<VehicleSaleItem>(true);
            if (items == null) return;

            foreach (var item in items)
            {
                if (item != null && item.VehicleLevel == gameVehicleID)
                {
                    VehicleUiHelper.ForceArchipelagoVisuals(item);
                    break;
                }
            }
        }

        /// <summary>
        /// Update Storage UI
        /// </summary>
        public static void RefreshStorageUI()
        {

                var storageItems = GameObject.FindObjectsOfType<StorageSectionItem>(true);
                if (storageItems == null) return;

                foreach (var item in storageItems)
                {
                    if (item != null)
                    {
                        item.CheckRequirements();
                    }
                }

        }

        /// <summary>
        /// Update Loan UI Catalog
        /// </summary>
        public static void RefreshLoanUI()
        {
            try
            {
                var loanItems = GameObject.FindObjectsOfType<LoanItem>(true);
                if (loanItems == null) return;

                foreach (var item in loanItems)
                {
                    if (item != null)
                    {
                        item.CheckIfReachedRequiredLevel(true);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"Error refreshing Loan UI: {ex.Message}");
            }
        }

        /// <summary>
        /// Update Vending UI Catalog
        /// </summary>
        public static void RefreshVendingUI()
        {
            try
            {
                var tab = GameObject.FindObjectOfType<__Project__.Scripts.Computer.Vending_Machine.VendingMachineTab>();
                if (tab != null && tab.gameObject.activeInHierarchy)
                {
                    var method = typeof(__Project__.Scripts.Computer.Vending_Machine.VendingMachineTab)
                        .GetMethod("OnEnableControl", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);

                    method?.Invoke(tab, null);
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"Error refreshing Vending UI: {ex.Message}");
            }
        }

        /// <summary>
        /// Trigger Power Outage Trap Event
        /// </summary>
        public static void TriggerPowerOutageEvent(float durationSeconds)
        {
            if (PowerOutageManager.Instance != null)
            {
                PowerOutageManager.Instance.StartPowerOutage(durationSeconds);
            }
        }

        /// <summary>
        /// Update Localization
        /// </summary>
        public static void RefreshLocalization(TMPro.TMP_Text textComponent)
        {
            if (textComponent == null) return;

            var localizeEvent = textComponent.GetComponent<LocalizeStringEvent>();

            if (localizeEvent == null && textComponent.transform.parent != null)
            {
                localizeEvent = textComponent.transform.parent.GetComponent<LocalizeStringEvent>();
            }

            if (localizeEvent != null)
            {
                localizeEvent.RefreshString();
            }
            else
            {
                if (textComponent.text.Contains("Locked"))
                {
                    textComponent.text = "Buy";
                }
            }
        }
    }
}