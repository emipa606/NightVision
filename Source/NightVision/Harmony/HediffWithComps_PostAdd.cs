using HarmonyLib;
using Verse;

namespace NightVision.Harmony;

[HarmonyPatch(typeof(HediffWithComps), nameof(HediffWithComps.PostAdd))]
public static class HediffWithComps_PostAdd
{
    public static void Postfix(
        HediffWithComps __instance
    )
    {
        if (__instance.pawn is { Spawned: true } pawn
            && pawn.TryGetComp<Comp_NightVision>() is { } comp
           )
        {
            comp.CheckAndAddHediff(__instance, __instance.Part);
        }
    }
}