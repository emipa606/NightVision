using HarmonyLib;
using RimWorld;
using Verse;

namespace NightVision.Harmony;

[HarmonyPatch(typeof(Pawn_ApparelTracker), "TakeWearoutDamageForDay")]
public static class ApparelTracker_TakeWearoutDamage
{
    public static void Postfix(
        Thing ap,
        Pawn_ApparelTracker __instance
    )
    {
        if (!ap.Destroyed
            || __instance.pawn is not { } pawn
            || !pawn.RaceProps.Humanlike
            || pawn.TryGetComp<Comp_NightVision>() is not { } comp)
        {
            return;
        }

        if (ap is Apparel apparel)
        {
            comp.RemoveApparel(apparel);
        }
    }
}