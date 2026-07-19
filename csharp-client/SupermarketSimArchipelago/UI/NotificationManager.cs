using System;
using UnityEngine;
using UnityEngine.UI;

namespace SupermarketArchipelago
{
    public class ArchipelagoNotificationManager : MonoBehaviour
    {
        public static ArchipelagoNotificationManager Instance { get; private set; }

        public ArchipelagoNotificationManager(IntPtr handle) : base(handle) { }

        private GameObject _canvasGo;
        private GameObject _panelGo;
        private Text _textComponent;
        private CanvasGroup _canvasGroup;

        private enum AnimState { Inactive, FadeIn, Displaying, FadeOut }
        private AnimState _currentState = AnimState.Inactive;
        private float _displayTimer = 0f;

        public static void Create()
        {
            if (Instance != null) return;

            GameObject managerGo = new GameObject("ArchipelagoNotificationManager");
            DontDestroyOnLoad(managerGo);
            Instance = managerGo.AddComponent<ArchipelagoNotificationManager>();
            Instance.SetupUI();
        }

        private void SetupUI()
        {
            _canvasGo = new GameObject("AP_NotificationCanvas");
            _canvasGo.transform.SetParent(this.transform);

            Canvas canvas = _canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999; 

            _canvasGo.AddComponent<CanvasScaler>();
            _canvasGroup = _canvasGo.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f; 

            _panelGo = new GameObject("AP_NotificationPanel");
            _panelGo.transform.SetParent(_canvasGo.transform);

            Image bgImage = _panelGo.AddComponent<Image>();
            bgImage.color = new Color(0.05f, 0.05f, 0.05f, 0.9f); 

            RectTransform panelRect = _panelGo.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 1f); 
            panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
            panelRect.anchoredPosition = new Vector2(0, -25); 
            panelRect.sizeDelta = new Vector2(450, 65); 

            GameObject textGo = new GameObject("AP_NotificationText");
            textGo.transform.SetParent(_panelGo.transform);

            _textComponent = textGo.AddComponent<Text>();
            _textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            _textComponent.fontSize = 16;
            _textComponent.alignment = TextAnchor.MiddleCenter;
            _textComponent.color = Color.white;

            RectTransform textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = new Vector2(-20, -10); 
            textRect.anchoredPosition = Vector2.zero;

            _canvasGo.SetActive(false);
        }

        /// <summary>
        /// Show a notification message on the screen with optional color coding based on type.
        /// </summary>
        public void Show(string message, int type = 0)
        {
            Color color = Color.white;
            if (type == 1) color = new Color(1f, 0.3f, 0.3f); // Red (Traps)
            if (type == 2) color = new Color(0.3f, 0.7f, 1f); // Blue (Locations)
            if (type == 3) color = new Color(0.63f, 0.13f, 0.94f); // Purple (Items)
            if (type == 4) color = new Color(0.13f, 0.94f, 0.63f); // Green (Goal)

            _textComponent.text = message;
            _textComponent.color = color;

            
            _canvasGo.SetActive(true);
            _currentState = AnimState.FadeIn;
        }

        private void Update()
        {
            if (_currentState == AnimState.Inactive) return;

            switch (_currentState)
            {
                case AnimState.FadeIn:
                    _canvasGroup.alpha += Time.deltaTime * 5f; 
                    if (_canvasGroup.alpha >= 1f)
                    {
                        _canvasGroup.alpha = 1f;
                        _currentState = AnimState.Displaying;
                        _displayTimer = 4.5f; 
                    }
                    break;

                case AnimState.Displaying:
                    _displayTimer -= Time.deltaTime;
                    if (_displayTimer <= 0f)
                    {
                        _currentState = AnimState.FadeOut;
                    }
                    break;

                case AnimState.FadeOut:
                    _canvasGroup.alpha -= Time.deltaTime * 3f; 
                    if (_canvasGroup.alpha <= 0f)
                    {
                        _canvasGroup.alpha = 0f;
                        _canvasGo.SetActive(false);
                        _currentState = AnimState.Inactive;
                    }
                    break;
            }
        }
    }
}