﻿using HarmonyLib;
using RimWorld;
using Verse;

namespace NightVision.Harmony;

[HarmonyPatch(typeof(PawnRecentMemory), nameof(PawnRecentMemory.RecentMemoryInterval))]
public static class PawnRecentMemory_RecentMemoryInterval
{
    public static void Postfix(Pawn ___pawn)
    {
        if (___pawn?.Map != null && ___pawn.Map.glowGrid.PsychGlowAt(___pawn.Position) != PsychGlow.Overlit)
        {
            ThoughtWorker_TooBright.SetLastDarkTick(___pawn);
        }
    }
}