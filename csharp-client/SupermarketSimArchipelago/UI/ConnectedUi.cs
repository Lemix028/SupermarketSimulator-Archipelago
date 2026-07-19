using UnityEngine;
using UnityEngine.UI;

namespace SupermarketArchipelago
{
    public class ConnectedUi : MonoBehaviour
    {
        private static bool _showLoginWindow = false;
        private static string _serverUrlInput = "";
        private static string _slotNameInput = "";
        private static string _passwordInput = "";
        private static int _focusedFieldId = 0; // 0 = URL, 1 = Slot, 2 = Password
        
        private static bool _pendingEventSystemEnable = false;
        private static System.Collections.Generic.List<Button> _deactivatedButtons = new System.Collections.Generic.List<Button>();
        private Rect _windowRect;

        private void Start()
        {
            // Center the window (width = 1000, height = 600)
            _windowRect = new Rect(Screen.width / 2 - 500, Screen.height / 2 - 300, 1000, 600);
        }

        public static void OpenLoginWindow()
        {
            _serverUrlInput = MainMenuPatch.ConfigConfigUrl.Value;
            _slotNameInput = MainMenuPatch.ConfigSlotName.Value;
            _passwordInput = MainMenuPatch.ConfigPassword.Value;
            _focusedFieldId = 0; // Default focus to Server URL
            _showLoginWindow = true;
            _pendingEventSystemEnable = false; // Reset flag

            SetMainMenuButtonsActive(false);
        }

        private void OnDisable()
        {
            SetMainMenuButtonsActive(true);
        }

        private static void SetMainMenuButtonsActive(bool active)
        {
            if (active)
            {
                foreach (var btn in _deactivatedButtons)
                {
                    if (btn != null)
                    {
                        btn.interactable = true;
                    }
                }
                _deactivatedButtons.Clear();
            }
            else
            {
                _deactivatedButtons.Clear();
                var buttons = FindObjectsOfType<Button>();
                foreach (var btn in buttons)
                {
                    // Deactivate all buttons in the main menu except our own Archipelago connect button
                    if (btn != null && btn.gameObject.name != "Archipelago_Connect_Button" && btn.interactable)
                    {
                        btn.interactable = false;
                        _deactivatedButtons.Add(btn);
                    }
                }
            }
        }

        private void Update()
        {
            if (_pendingEventSystemEnable)
            {
                // ONLY re-enable the menu buttons when the user has fully released the mouse button.
                // This guarantees that the closing click cannot propagate to background buttons.
                if (!Input.GetMouseButton(0))
                {
                    SetMainMenuButtonsActive(true);
                    _pendingEventSystemEnable = false;
                }
            }

            if (!_showLoginWindow || _focusedFieldId == -1) return;

            // Handle Ctrl+V Paste
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                if (Input.GetKeyDown(KeyCode.V))
                {
                    string pasteText = GUIUtility.systemCopyBuffer;
                    if (!string.IsNullOrEmpty(pasteText))
                    {
                        AppendText(pasteText);
                    }
                    return;
                }
            }

            // Handle Backspace
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                RemoveLastChar();
                return;
            }

            // Handle Tab to cycle focus
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                _focusedFieldId = (_focusedFieldId + 1) % 3;
                return;
            }

            // Handle typing characters
            string typed = Input.inputString;
            if (!string.IsNullOrEmpty(typed))
            {
                AppendText(typed);
            }
        }

        private void AppendText(string append)
        {
            foreach (char c in append)
            {
                if (c == '\b' || c == '\n' || c == '\r' || c == '\t') continue; // Skip control characters
                
                if (_focusedFieldId == 0) _serverUrlInput += c;
                else if (_focusedFieldId == 1) _slotNameInput += c;
                else if (_focusedFieldId == 2) _passwordInput += c;
            }
        }

        private void RemoveLastChar()
        {
            if (_focusedFieldId == 0 && _serverUrlInput.Length > 0)
                _serverUrlInput = _serverUrlInput.Substring(0, _serverUrlInput.Length - 1);
            else if (_focusedFieldId == 1 && _slotNameInput.Length > 0)
                _slotNameInput = _slotNameInput.Substring(0, _slotNameInput.Length - 1);
            else if (_focusedFieldId == 2 && _passwordInput.Length > 0)
                _passwordInput = _passwordInput.Substring(0, _passwordInput.Length - 1);
        }

        private void OnGUI()
        {
            // Draw connection status HUD in the corner
            DrawConnectionHUD();

            // Draw login window if active
            if (_showLoginWindow)
            {
                // Re-center window dynamically in case screen size changed (width = 1000, height = 600)
                _windowRect = new Rect(Screen.width / 2 - 500, Screen.height / 2 - 300, 1000, 600);
                
                // Save original colors
                Color originalBg = GUI.backgroundColor;
                Color originalColor = GUI.color;

                // Decrease transparency: alpha 1.0f (opaque)
                GUI.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 1f);
                GUI.color = Color.white;

                _windowRect = GUI.Window(0, _windowRect, (System.Action<int>)DrawLoginWindow, "Archipelago Connection Details");

                // Restore colors
                GUI.backgroundColor = originalBg;
                GUI.color = originalColor;
            }
        }

        private void DrawConnectionHUD()
        {
            Rect uiRect = new Rect(8, 8, 110, 22);

            GUI.backgroundColor = new Color(0f, 0f, 0f, 0.85f);
            GUI.Box(uiRect, "");

            GUIStyle textStyle = GUI.skin.label;
            TextAnchor originalAlignment = textStyle.alignment;
            FontStyle originalFontStyle = textStyle.fontStyle;
            Color originalColor = GUI.color;
            Color originalBgColor = GUI.backgroundColor;

            textStyle.alignment = TextAnchor.MiddleCenter;
            textStyle.fontStyle = FontStyle.Bold;

            if (ArchipelagoClient.IsConnected)
            {
                GUI.color = Color.green;
                GUI.Label(uiRect, "Connected", textStyle);
            }
            else
            {
                GUI.color = Color.red;
                GUI.Label(uiRect, "Disconnected", textStyle);
            }

            textStyle.alignment = originalAlignment;
            textStyle.fontStyle = originalFontStyle;
            GUI.color = originalColor;
            GUI.backgroundColor = originalBgColor;
        }

        private void DrawLoginWindow(int windowId)
        {
            // Save original styles
            FontStyle originalFontStyle = GUI.skin.label.fontStyle;
            int originalLabelSize = GUI.skin.label.fontSize;
            int originalButtonSize = GUI.skin.button.fontSize;
            int originalBoxSize = GUI.skin.box.fontSize;

            // Set new styles for scaling
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUI.skin.label.fontSize = 26;
            GUI.skin.button.fontSize = 24;
            GUI.skin.box.fontSize = 22;

            // 1. Server Address (Label at y=60, TextField at y=100)
            GUI.Label(new Rect(80, 60, 840, 36), "Server Address (Host:Port):");
            DrawCustomTextField(new Rect(80, 100, 840, 54), ref _serverUrlInput, 0, false);

            // 2. Slot Name (Label at y=190, TextField at y=230)
            GUI.Label(new Rect(80, 190, 840, 36), "Slot Name:");
            DrawCustomTextField(new Rect(80, 230, 840, 54), ref _slotNameInput, 1, false);

            // 3. Password (Label at y=320, TextField at y=360)
            GUI.Label(new Rect(80, 320, 840, 36), "Password (leave blank if none):");
            DrawCustomTextField(new Rect(80, 360, 840, 54), ref _passwordInput, 2, true);

            // Restore label styles before drawing buttons
            GUI.skin.label.fontStyle = originalFontStyle;
            GUI.skin.label.fontSize = originalLabelSize;

            // Action Buttons (at y=480, height=70, width=360)
            if (GUI.Button(new Rect(80, 480, 360, 70), "Connect & Save"))
            {
                // Save values
                MainMenuPatch.ConfigConfigUrl.Value = _serverUrlInput.Trim();
                MainMenuPatch.ConfigSlotName.Value = _slotNameInput.Trim();
                MainMenuPatch.ConfigPassword.Value = _passwordInput;
                
                if (Plugin.ConfigFileInstance != null)
                {
                    Plugin.ConfigFileInstance.Save();
                }

                // Connect
                ArchipelagoClient.Connect(
                    MainMenuPatch.ConfigConfigUrl.Value,
                    MainMenuPatch.ConfigSlotName.Value,
                    string.IsNullOrEmpty(MainMenuPatch.ConfigPassword.Value) ? null : MainMenuPatch.ConfigPassword.Value
                );

                _showLoginWindow = false;
                _pendingEventSystemEnable = true; // Delay activation until mouse button is released
            }

            if (GUI.Button(new Rect(560, 480, 360, 70), "Cancel"))
            {
                _showLoginWindow = false;
                _pendingEventSystemEnable = true; // Delay activation until mouse button is released
            }

            // Restore remaining GUI skin settings
            GUI.skin.button.fontSize = originalButtonSize;
            GUI.skin.box.fontSize = originalBoxSize;
        }

        private void DrawCustomTextField(Rect rect, ref string text, int fieldId, bool isPassword)
        {
            bool isFocused = _focusedFieldId == fieldId;

            Color originalBg = GUI.backgroundColor;
            
            // Set style based on focus
            if (isFocused)
            {
                GUI.backgroundColor = new Color(0.2f, 0.6f, 0.9f, 1f); // Blue highlight
            }
            else
            {
                GUI.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 1f);
            }

            // Click detection via invisible button overlay
            if (GUI.Button(rect, ""))
            {
                _focusedFieldId = fieldId;
            }

            // Draw text box container
            GUI.Box(rect, "");
            GUI.backgroundColor = originalBg;

            // Generate display string
            string displayText = text;
            if (isPassword)
            {
                displayText = new string('*', text.Length);
            }

            // Append blinking cursor
            if (isFocused)
            {
                if ((int)(Time.time * 2) % 2 == 0)
                {
                    displayText += "|";
                }
            }

            // Draw text inside
            TextAnchor originalAlignment = GUI.skin.label.alignment;
            int originalLabelSize = GUI.skin.label.fontSize;
            
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUI.skin.label.fontSize = 22; // Font size of input text
            
            Rect textRect = new Rect(rect.x + 20, rect.y, rect.width - 40, rect.height);
            GUI.Label(textRect, displayText);
            
            GUI.skin.label.alignment = originalAlignment;
            GUI.skin.label.fontSize = originalLabelSize;
        }
    }
}