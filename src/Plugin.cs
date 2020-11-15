using BepInEx;
using System;
using System.Linq;
using UnityEngine;

namespace RewiredIMGUIBlocker
{
    [BepInPlugin(PluginInfo.PLUGIN_ID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        const string REWIRED_ASSEMBLY_NAME = "Rewired_Core";

        static bool hasRewired = false;
        static bool shouldReenableKeyboard = false;

        void Awake()
        {
            var rewiredAssembly = AppDomain.CurrentDomain
                .GetAssemblies()
                .SingleOrDefault(assembly => assembly.GetName().Name.Equals(REWIRED_ASSEMBLY_NAME, StringComparison.OrdinalIgnoreCase));

            hasRewired = rewiredAssembly != null;

            if (hasRewired) Logger.LogInfo("Rewired found.");
            else Logger.LogWarning("Rewired NOT found. Going to sleep.");
        }

        void OnGUI()
        {
            if (!hasRewired) return;

            PerformBlock();
        }

        // Leave this in a separate method to avoid problems if Rewired is not loaded
        static void PerformBlock()
        {
            var canBeEnabled = GUIUtility.keyboardControl == 0;
            var keyboard = Rewired.ReInput.controllers.Keyboard;

            if (keyboard.enabled == canBeEnabled) return;
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
    }
}
