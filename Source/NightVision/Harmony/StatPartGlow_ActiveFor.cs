﻿// NightVision StatPartGlow_ActiveFor.cs
// 
// 17 10 2018
// 
// 17 10 2018

using HarmonyLib;
using RimWorld;
using Verse;

namespace NightVision.Harmony;

// TODO RW1.3 added further checks for blindness and for prefering darkness - may want to incorporate the same into the nightvision comp
[HarmonyPatch(typeof(StatPart_Glow), "ActiveFor")]
public static class StatPartGlow_ActiveFor
{
    [HarmonyPostfix]
    public static void Postfix(
        ref Thing t,
        ref bool __result
    )
    {
        if (__result || !t.Spawned)
        {
            return;
        }

        if (t is Pawn pawn && pawn.TryGetComp<Comp_NightVision>() != null &&
            (!ModLister.BiotechInstalled || !pawn.genes.HasActiveGene(NVStatWorker.DarkVision)))
        {
            __result = true;
        }
    }
}