using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace TooltipUnits
{
    public class TU : AbstractMod, IModSettings
    {
        public const string VERSION_NUMBER = "v1.0";

        public override string getIdentifier() => "TooltipUnits";

        public override string getName() => "Coaster Builder: Metric & Imperial Height Units";

        public override string getDescription() => @"Mod Inventor: RavenStryker" + Environment.NewLine + "Project Author: ChrisBradel" + Environment.NewLine + "Project Upkeep/Modifications: RavenStryker" + Environment.NewLine + "Description: Shows height marker tooltips in feet or meters while using the coaster builder. Selecting Metric Units within the game settings will show you meters, selecting Imperial will show you feet.";

        public override string getVersionNumber() => VERSION_NUMBER;

        public override bool isMultiplayerModeCompatible() => true;

        public override bool isRequiredByAllPlayersInMultiplayerMode() => false;

        public GameObject go { get; private set; }
        public static TU Instance;
        private Harmony _harmony;

        private static bool debug_mode = false;
        public static bool MOD_ENABLED = false;

        public static string _local_mods_directory = "";

        public static void TUDebug(string debug_string, bool always_show = false)
        {
            if (debug_mode || always_show)
            {
                Debug.LogWarning("Tooltip Units: " + debug_string);

                if (_local_mods_directory != "")
                    File.AppendAllText(_local_mods_directory + "/TooltipUnits.txt", "Tooltip Units: " + debug_string + "\n");
            }

            if (File.Exists(_local_mods_directory + "/tu_debug"))
            {
                debug_mode = true;
                TUDebug("Found debug flag file.");
            }
        }

        public TU()
        {
            _local_mods_directory = GameController.modsPath;

            if (!MOD_ENABLED)
            {
                _harmony = new Harmony(getIdentifier());
                _harmony.PatchAll();
                MOD_ENABLED = true;
                TUDebug(debug_string: "ENABLING TU", always_show: true);
            }
        }

        public override void onEnabled()
        {
            Instance = this;

            if (!MOD_ENABLED)
            {
                _harmony = new Harmony(getIdentifier());
                _harmony.PatchAll();
                MOD_ENABLED = true;
                TUDebug(debug_string: "ENABLING TU", always_show: true);
            }

            TUDebug("Modspath: " + GameController.modsPath, always_show: true);
            TUDebug("ModspathRel: " + GameController.modsPathRelative, always_show: true);

            go = new GameObject();
            go.name = "TU GameObject";
        }

        public override void onDisabled()
        {
            UnityEngine.Object.DestroyImmediate(go);

            if (MOD_ENABLED)
            {
                _harmony.UnpatchAll(getIdentifier());
                MOD_ENABLED = false;
                TUDebug(debug_string: "DISABLING TU", always_show: true);
            }
        }

        public void onDrawSettingsUI()
        {
        }

        public void onSettingsClosed()
        {
        }

        public void onSettingsOpened()
        {
        }
    }

    [HarmonyPatch]
    public class UnitPatch
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Trust me, it's used. It's found by Harmony and not referred to by this mod directly.")]
        private static MethodBase TargetMethod() => AccessTools.Method(typeof(TextUtility), "formatHeightmark", parameters: new Type[]
        {
            typeof(float)
        });

        [HarmonyPrefix]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "Cuz I said so! (Also it thinks __result is unused even though it isn't.)")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "This is how Parkitect capitalizes it, so it has to match.")]
        public static bool formatHeightmark(ref string __result, float height)
        {
            if (Settings.Instance.unitSystem != Settings.UnitSystem.IMPERIAL)
            {
                float newHeight = Mathf.Round(height * 3f - 9f);
                __result = $"{newHeight} m";
            }
            else
            {
                float newHeight = Mathf.Round(height * 3f / 0.3048006f - 30f);
                __result = $"{newHeight} ft";
            }
            return false;
        }
    }
}