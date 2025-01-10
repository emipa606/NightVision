using HarmonyLib;
using Verse;

namespace NightVision.Harmony;

[HarmonyPatch(typeof(Hediff), nameof(Hediff.PostAdd))]
public static class Hediff_PostAdd
{
    public static void Postfix(
        Hediff __instance
    )
    {
        if (__instance?.pawn is { Spawned: true } pawn /*&& pawn.RaceProps.Humanlike*/
            && pawn.TryGetComp<Comp_NightVision>() is { } comp)
        {
            comp.CheckAndAddHediff(__instance, __instance.Part);
        }
    }
}