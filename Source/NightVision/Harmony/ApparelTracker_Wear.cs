using HarmonyLib;
using RimWorld;
using Verse;

namespace NightVision.Harmony;

[HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Wear))]
public static class ApparelTracker_Wear
{
    public static void Postfix(
        Apparel newApparel,
        Pawn_ApparelTracker __instance
    )
    {
        if (__instance?.pawn is not { Spawned: true } pawn || !pawn.RaceProps.Humanlike)
        {
            return;
        }

        if (pawn.TryGetComp<Comp_NightVision>() is { } comp)
        {
            comp.CheckAndAddApparel(newApparel);
        }
    }
}