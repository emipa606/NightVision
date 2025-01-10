using HarmonyLib;
using Verse;

namespace NightVision.Harmony;

//HediffWithComps - because this class, derived from Hediff, doesn't use base.PostAdd

[HarmonyPatch(typeof(Hediff), nameof(Hediff.PostRemoved))]
public static class Hediff_PostRemoved
{
    public static void Postfix(
        Hediff __instance
    )
    {
        if (__instance?.pawn is { Spawned: true } pawn /*&& pawn.RaceProps.Humanlike*/
            && pawn.TryGetComp<Comp_NightVision>() is { } comp)
        {
            comp.RemoveHediff(__instance, __instance.Part);
        }
    }
}