﻿using HarmonyLib;
using UnityEngine;
using Verse;

namespace NightVision.Harmony;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.BodySize), MethodType.Getter)]
public static class Pawn_BodySize
{
    public static void Postfix(
        ref Pawn __instance,
        ref float __result
    )
    {
        //Currently not used anywhere. Future plans
        var modextension = __instance?.def?.GetModExtension<Stealth_ModExtension>();

        if (modextension == null || !__instance.Spawned)
        {
            return;
        }

        if (__instance.Map.glowGrid.GroundGlowAt(__instance.Position) > 0.3f)
        {
            return;
        }

        __result *= Mathf.Lerp(
            modextension.lowlightbodysizefactor,
            1,
            __instance.Map.glowGrid.GroundGlowAt(__instance.Position) / 0.3f
        );
    }
}