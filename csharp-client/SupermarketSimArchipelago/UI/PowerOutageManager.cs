using System;
using System.Collections.Generic;
using UnityEngine;

namespace SupermarketArchipelago
{
    public class PowerOutageManager : MonoBehaviour
    {
        public static PowerOutageManager Instance { get; private set; }

        public PowerOutageManager(IntPtr handle) : base(handle) { }

        private float _powerOutageTimer = 0f;
        private List<Light> _disabledLights = null;

        public static void Create()
        {
            if (Instance != null) return;

            GameObject managerGo = new GameObject("ArchipelagoPowerOutageManager");
            DontDestroyOnLoad(managerGo);
            Instance = managerGo.AddComponent<PowerOutageManager>();
        }

        public void StartPowerOutage(float durationSeconds)
        {
            var lights = GameObject.FindObjectsOfType<Light>(true);
            if (lights == null || lights.Length == 0) return;

            if (_disabledLights == null)
            {
                _disabledLights = new List<Light>();
            }

            foreach (var light in lights)
            {
                if (light != null && light.type != LightType.Directional && light.enabled)
                {
                    _disabledLights.Add(light);
                    light.enabled = false;
                }
            }

            _powerOutageTimer = durationSeconds;
        }

        private void Update()
        {
            if (_disabledLights != null && _disabledLights.Count > 0)
            {
                _powerOutageTimer -= Time.deltaTime;
                if (_powerOutageTimer <= 0f)
                {
                    foreach (var light in _disabledLights)
                    {
                        if (light != null)
                        {
                            light.enabled = true;
                        }
                    }
                    _disabledLights.Clear();
                }
            }
        }
    }
}
