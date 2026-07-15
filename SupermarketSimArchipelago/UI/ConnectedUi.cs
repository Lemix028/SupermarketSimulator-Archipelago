using UnityEngine;

namespace SupermarketArchipelago
{
    public class ConnectedUi : MonoBehaviour
    {
        private void OnGUI()
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
    }
}