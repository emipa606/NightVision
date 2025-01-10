using RimWorld;
using Verse;

namespace NightVision.Harmony.Manual;

public static class ApparelTracker_TryDrop
{
    public static void Postfix(
        Apparel ap,
        Pawn_ApparelTracker __instance,
        ref bool __result
    )
    {
        if (__result
            && __instance?.pawn is { Spawned: true } pawn
            && pawn.RaceProps.Humanlike
            && pawn.TryGetComp<Comp_NightVision>() is { } comp)
        {
            comp.RemoveApparel(ap);
        }
    }
}