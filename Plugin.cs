using BepInEx;
using HarmonyLib;
using System.Reflection;
using System;
using UnityEngine.Rendering;
using BoplFixedMath;
using BepInEx.Configuration;
using UnityEngine;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Reflection.Emit;
using Mono.Cecil.Cil;
using System.Linq;
using BepInEx.Logging;

namespace BetterTimeStop
{
    [BepInPlugin("com.maxgamertyper1.bettertimestop", "Better Time Stop", "1.0.0")]
    public class BetterTimeStop : BaseUnityPlugin
    {
        internal static ConfigFile config;
        internal static ConfigEntry<int> MinimumDuration;
        internal static ConfigEntry<int> MaximumDuration;
        internal static ConfigEntry<bool> MaximumOverride;
        public void Log(string message)
        {
            Logger.LogInfo(message);
        }

        private void Awake()
        {
            // Plugin startup logic
            Log($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            DoPatching();

            config = ((BaseUnityPlugin)this).Config;
            MinimumDuration = config.Bind<int>("Duration", "MinimumDuration", 5, "The Minimum duration of the TimeStop (Vanilla is 10)");
            MaximumDuration = config.Bind<int>("Duration", "MaximumDuration", 15, "The Maximum duration of the TimeStop (Vanilla is 10)");
            Patches.MaximumDuration = new Fix(MaximumDuration.Value);
            Patches.MinimumDuration = new Fix(MinimumDuration.Value);
        }

        private void DoPatching()
        {
            var harmony = new Harmony("com.maxgamertyper1.bettertimestop");

            Patch(harmony, typeof(CastSpell), "Awake", "MinDurationPatch", false,false);
            Patch(harmony, typeof(CastSpell), "Update", "UpdateTimeData", false, false);
            //Patch(harmony, typeof(CastSpell), "UpdateSim", "UpdateSimOverride", true, false);
            Patch(harmony, typeof(CastSpell), "UpdateSim", "IfStatementOverride", false, true);
            Patch(harmony, typeof(TimeStop), "Init", "DurationPatch", false, false);
        }

        private void OnDestroy()
        {
            Log($"Bye Bye From {PluginInfo.PLUGIN_GUID}");
        }

        private void Patch(Harmony harmony, Type OriginalClass , string OriginalMethod, string PatchMethod, bool prefix, bool transpiler)
        {
            MethodInfo MethodToPatch = AccessTools.Method(OriginalClass, OriginalMethod); // the method to patch
            MethodInfo Patch = AccessTools.Method(typeof(Patches), PatchMethod);
            
            if (prefix)
            {
                harmony.Patch(MethodToPatch, new HarmonyMethod(Patch));
            }
            else
            {
                if (transpiler)
                {
                    harmony.Patch(MethodToPatch, null, null, new HarmonyMethod(Patch));
                } else
                {
                    harmony.Patch(MethodToPatch, null, new HarmonyMethod(Patch));
                }
            }
            Log($"Patched {OriginalMethod} in {OriginalClass.ToString()}");
        }
    }

    public class Patches
    {
        public static Fix MaximumDuration;
        public static Fix MinimumDuration;
        static Dictionary<int,Fix> TimeData = new Dictionary<int, Fix>();
        public static void MinDurationPatch(ref CastSpell __instance)
        {
            if (__instance.spell.name=="TimeStopSphere")
            {
                __instance.castTime = MinimumDuration;
            }
        }

        static IEnumerable<CodeInstruction> IfStatementOverride(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == System.Reflection.Emit.OpCodes.Ldfld // if its a load to the stack
                && codes[i].operand is FieldInfo fieldInfo // if it has field info
                && fieldInfo.Name == "castTime" // if the variable to load is castTime
                && fieldInfo.DeclaringType == typeof(CastSpell)) // if the class is CastSpell
                {
                    System.Diagnostics.Debug.Print("Found load to stack and replaced with MaximumDuration");

                    var maximumDurationField = typeof(Patches).GetField("MaximumDuration", BindingFlags.Static | BindingFlags.Public); // get the maximum duration field
                    codes[i] = new CodeInstruction(System.Reflection.Emit.OpCodes.Ldsfld, maximumDurationField);  // Load MaximumDuration as a Fix constant
                    codes.RemoveAt(i - 1);

                    System.Diagnostics.Debug.Print("Printing BetterTimeStop Transpiler Change: (used for debuging in errors)");
                    System.Diagnostics.Debug.Print("" + codes[i-2]);
                    System.Diagnostics.Debug.Print("" + codes[i-1]);
                    System.Diagnostics.Debug.Print("" + codes[i]);
                    System.Diagnostics.Debug.Print("" + codes[i + 1]);
                    System.Diagnostics.Debug.Print("Finished Printing BetterTimeStop Transpiler Change");

                    break;
                }
            }

            return codes;
        }

        public static void UpdateTimeData(ref CastSpell __instance)
        {
            TimeData[__instance.playerInfo.playerId] = __instance.timeSinceActivation;
        }

        public static void DurationPatch(ref TimeStop __instance)
        {
            __instance.duration = (TimeData[__instance.GetComponent<IPlayerIdHolder>().GetPlayerId()] > MaximumDuration)  ? (float)MaximumDuration : (float)TimeData[__instance.GetComponent<IPlayerIdHolder>().GetPlayerId()];
        }
    }
}
