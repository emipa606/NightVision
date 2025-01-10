using HarmonyLib;
using RimWorld;
using Verse;

namespace NightVision.Harmony;

[HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Remove))]
public static class ApparelTracker_Remove
{
    public static void Postfix(
        Apparel ap,
        Pawn_ApparelTracker __instance
    )
    {
        if (__instance?.pawn is { } pawn
            && pawn.RaceProps.Humanlike
            && pawn.Spawned
            && pawn.TryGetComp<Comp_NightVision>() is { } comp)
        {
            comp.RemoveApparel(ap);
        }
    }
}