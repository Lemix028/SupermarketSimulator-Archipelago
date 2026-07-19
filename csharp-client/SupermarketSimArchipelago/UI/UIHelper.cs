#nullable disable
using SupermarketSimArchipelago;
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

        private static Dictionary<Light, float> _lightsToRestore;
        /// <summary>
        /// Update License UI
        /// </summary>
        public static void RefreshLicenseUI(int licenseID)
        {
            var tab = GameObject.FindObjectOfType<LicensesTab>();
            if (tab == null) return;

            foreach (var item in tab.m_LicenseItems)
            {
                if (item.m_licenseID == licenseID)
                {
                    // Set active status and level requirements
                    item.OnLicenseActivatedExternally(licenseID, true);
                    item.CheckIfReachedRequiredLevel(true);
                    item.RefreshFromState();

                    // Restore localization and enable purchase interaction
                    RefreshLocalization(item.m_PurchaseButtonText);
                    if (item.m_PurchaseButton != null) item.m_PurchaseButton.interactable = true;

                    break;
                }
            }
            tab.ApplyFiltersAndSort();
        }

        /// <summary>
        /// Update Section UI
        /// </summary>
        public static void RefreshSectionUI(int gameSectionID)
        {
            // Find all active section items in the current UI menu
            var items = GameObject.FindObjectsOfType<GrowthSectionItem>();
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
            var items = GameObject.FindObjectsOfType<FurnitureSalesItem>();
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
            var items = GameObject.FindObjectsOfType<VehicleSaleItem>();
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

                var storageItems = GameObject.FindObjectsOfType<StorageSectionItem>();
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
                var loanItems = GameObject.FindObjectsOfType<LoanItem>();
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

            var lights = GameObject.FindObjectsOfType<Light>();
            if (lights == null) return;

            var disabledLights = new Dictionary<Light, float>();

            foreach (var light in lights)
            {
                if (light != null && light.type != LightType.Directional)
                {
                    disabledLights[light] = light.intensity;
                    light.intensity = 0f;
                }
            }

            Timer timer = new Timer(
                new TimerCallback(OnPowerOutageTimerElapsed),
                disabledLights,
                (int)(durationSeconds * 1000),
                Timeout.Infinite
            );
        }

        /// <summary>
        /// Explicit callback method executed when the background timer runs out
        /// </summary>
        private static void OnPowerOutageTimerElapsed(object state)
        {
            _lightsToRestore = state as Dictionary<Light, float>;
            if (_lightsToRestore == null) return;

            UnityMainThreadDispatcher.Enqueue(new Action(RestoreLightsOnMainThread));
        }

        /// <summary>
        /// Safe execution wrapper running back on Unity's Main Thread loop
        /// </summary>
        private static void RestoreLightsOnMainThread()
        {
            if (_lightsToRestore == null) return;

            foreach (var kvp in _lightsToRestore)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.intensity = kvp.Value; 
                }
            }

            _lightsToRestore = null; // Clear allocation memory safely
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