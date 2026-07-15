using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using BepInEx.Configuration;
using TMPro;
using UnityEngine.Localization.Components;

namespace SupermarketArchipelago
{
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public static class MainMenuPatch
    {
        public static ConfigEntry<string> ConfigConfigUrl;
        public static ConfigEntry<string> ConfigSlotName;
        public static ConfigEntry<string> ConfigPassword;

        private static TextMeshProUGUI _apButtonText;
        private static Button _apButton;

        [HarmonyPostfix]
        public static void Postfix(MainMenuManager __instance)
        {
            if (__instance == null) return;

            Button baseButton = __instance.m_LoadButton ?? __instance.m_ContinueButton;
            if (baseButton == null) return;

            GameObject apBtnObject = Object.Instantiate(baseButton.gameObject, baseButton.transform.parent);
            apBtnObject.name = "Archipelago_Connect_Button";

            RectTransform rect = apBtnObject.GetComponent<RectTransform>();
            if (rect != null) rect.anchoredPosition = new Vector2(rect.anchoredPosition.x + 420f, rect.anchoredPosition.y - 30f);

            var localizer = apBtnObject.GetComponent<LocalizeStringEvent>() ?? apBtnObject.GetComponentInChildren<LocalizeStringEvent>();
            if (localizer != null)
            {
                localizer.OnUpdateString.RemoveAllListeners();
                localizer.enabled = false;
            }

            _apButtonText = apBtnObject.GetComponentInChildren<TextMeshProUGUI>();
            _apButton = apBtnObject.GetComponent<Button>(); 

            RefreshButtonText();

            if (_apButton != null)
            {
                _apButton.onClick.RemoveAllListeners();
                for (int i = 0; i < _apButton.onClick.GetPersistentEventCount(); i++)
                {
                    _apButton.onClick.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
                }

                _apButton.onClick.AddListener(new System.Action(() =>
                {

                    if (!ArchipelagoClient.IsConnected)
                    {
                        if (_apButtonText != null)
                        {
                            _apButtonText.text = "Connecting...";
                            _apButtonText.color = Color.cyan;
                        }

                        string url = ConfigConfigUrl.Value;
                        string slot = ConfigSlotName.Value;
                        string pass = ConfigPassword.Value;

                        ArchipelagoClient.Connect(url, slot, string.IsNullOrEmpty(pass) ? null : pass);
                    }
                }));
            }
        }


        public static void RefreshButtonText()
        {
            if (_apButtonText == null || _apButton == null) return;

            if (ArchipelagoClient.IsConnected)
            {
                _apButtonText.text = "Connected AP Server";
                _apButtonText.color = Color.green;
                _apButton.interactable = false; 
            }
            else
            {
                _apButtonText.text = "Connect AP Server";
                _apButtonText.color = Color.white;
                _apButton.interactable = true; 
            }
        }
    }
}