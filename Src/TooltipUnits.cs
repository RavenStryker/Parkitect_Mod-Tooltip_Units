using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace TooltipUnits
{
    public class TU : AbstractMod, IModSettings
    {
        public const string VERSION_NUMBER = "240404";

        public override string getIdentifier() => "TooltipUnits";

        public override string getName() => "Tooltip Units";

        public override string getDescription() => @"Shows height marker tooltips in feet or meters.";

        public override string getVersionNumber() => VERSION_NUMBER;

        public override bool isMultiplayerModeCompatible() => true;

        public override bool isRequiredByAllPlayersInMultiplayerMode() => false;

        public GameObject go { get; private set; }
        public static TU Instance;
        private Harmony _harmony;

        private static bool debug_mode = false;
        public static bool MOD_ENABLED = false;

        public static string _local_mods_directory = "";
        public static string _material_painter_directory = "";

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
        private static MethodBase TargetMethod() => AccessTools.Method(typeof(TextUtility), "formatHeightmark", parameters: new Type[]
        {
            typeof(float)
        });

        [HarmonyPrefix]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        public static bool formatHeightmark(ref string __result, float height)
        {
            if (Settings.Instance.unitSystem != Settings.UnitSystem.IMPERIAL)
            {
                float num = Mathf.Round(height * 3f - 9f);
                if (height - num / 3f - 0.0625f < 0f)
                {
                    __result = num.ToString() + " m";
                }

                __result = $"{Mathf.RoundToInt(num)} m";
            }
            else
            {
                float num = Mathf.Round(height * 3f / 0.3048006f - 30f);
                if (height - num * 0.3048006f / 3f - 0.0625f < 0f)
                {
                    __result = num.ToString() + " ft";
                }
                __result = $"{Mathf.RoundToInt(num)} ft";
            }

            return false;
        }
    }
}