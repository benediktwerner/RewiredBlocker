using BepInEx;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RewiredIMGUIBlocker
{
    [BepInPlugin(PluginInfo.PLUGIN_ID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        const string REWIRED_ASSEMBLY_NAME = "Rewired_Core";
        const string CONFIG_MANAGER_ASSEMBLY_NAME = "ConfigurationManager";

        static bool hasRewired = false;
        static bool shouldReenableKeyboard = false;

        private GameObject _clickBlockerCanvas;

        void Awake()
        {
            var rewiredAssembly = AppDomain.CurrentDomain
                .GetAssemblies()
                .SingleOrDefault(assembly => assembly.GetName().Name.Equals(REWIRED_ASSEMBLY_NAME, StringComparison.OrdinalIgnoreCase));

            hasRewired = rewiredAssembly != null;

            if (hasRewired) Logger.LogInfo("Rewired found.");
            else Logger.LogWarning("Rewired NOT found.");

        }

        void Start()
        {
            var configManagerAssembly = AppDomain.CurrentDomain
                .GetAssemblies()
                .SingleOrDefault(assembly => assembly.GetName().Name.Equals(CONFIG_MANAGER_ASSEMBLY_NAME, StringComparison.OrdinalIgnoreCase));

            if (configManagerAssembly == null)
            {
                Logger.LogInfo("BepInEx.ConfigurationManager is not loaded. Not blocking mouse input behind it.");
                return;
            }

            SetupClickBlocker();
        }

        void SetupClickBlocker()
        {
            var configManager = FindObjectOfType<ConfigurationManager.ConfigurationManager>();

            if (configManager == null)
            {
                Logger.LogInfo("No instance of BepInEx.ConfigurationManager found. Not blocking mouse input behind it.");
                return;
            }

            Logger.LogInfo("Found BepInEx.ConfigurationManager. Setting up mouse input block when opened.");
            configManager.DisplayingWindowChanged += (_, displayWindow) =>
            {
                BlockClicks(displayWindow.NewValue);
                if (hasRewired && !displayWindow.NewValue)
                {
                    GUIUtility.keyboardControl = 0;
                    BlockKeyboard(false);
                }
            };
        }

        void OnGUI()
        {
            if (!hasRewired) return;

            BlockKeyboard(GUIUtility.keyboardControl != 0);
        }

        // Leave this in a separate method to avoid problems if Rewired is not loaded
        static void BlockKeyboard(bool block)
        {
            var keyboard = Rewired.ReInput.controllers.Keyboard;

            if (keyboard.enabled != block) return;
            if (keyboard.enabled)
            {
                keyboard.enabled = false;
                shouldReenableKeyboard = true;
            }
            else if (shouldReenableKeyboard)
            {
                keyboard.enabled = true;
                shouldReenableKeyboard = false;
            }
        }

        private void BlockClicks(bool block)
        {
            if (block)
            {
                _clickBlockerCanvas = new GameObject("Rewired Click Blocker", typeof(Canvas), typeof(GraphicRaycaster));
                var canvas = _clickBlockerCanvas.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = Int16.MaxValue;
                DontDestroyOnLoad(_clickBlockerCanvas);
                var panel = new GameObject("Image", typeof(Image));
                panel.transform.SetParent(_clickBlockerCanvas.transform);
                var rect = panel.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 0);
                rect.anchorMax = new Vector2(1, 1);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                panel.GetComponent<Image>().color = new Color(0, 0, 0, 0.3f);
            }
            else if (_clickBlockerCanvas)
            {
                Destroy(_clickBlockerCanvas);
            }
        }
    }
}
