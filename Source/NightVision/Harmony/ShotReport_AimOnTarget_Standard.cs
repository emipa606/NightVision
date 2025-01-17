﻿using HarmonyLib;
using JetBrains.Annotations;
using Verse;

// ReSharper disable InconsistentNaming

namespace NightVision.Harmony;

[HarmonyPatch(typeof(ShotReport), nameof(ShotReport.AimOnTargetChance_StandardTarget))]
[HarmonyPatch(MethodType.Getter)]
public static class ShotReport_AimOnTargetChanceStandardTarget
{
    [HarmonyPostfix]
    [UsedImplicitly]
    public static void Postfix(
        //ref ShotReport __instance,
        ref float __result
    )
    {
        if (CurrentShot.NoShot || !CurrentShot.GlowFactor.FactorIsNonTrivial())
        {
            return;
        }

        CurrentShot.OriginalHitOnStandardTarget = __result;
        CurrentShot.ModifiedHitOnStandardTarget =
            CombatHelpers.HitChanceGlowTransform(__result, CurrentShot.GlowFactor);
        __result = CurrentShot.ModifiedHitOnStandardTarget;
    }
}