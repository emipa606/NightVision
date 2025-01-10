using Verse;

namespace NightVision.Harmony.Manual;

public static class PawnHealthTracker_AddHediff
{
    public static void AddHediff_Postfix(Hediff hediff, BodyPartRecord part, Pawn ___pawn)
    {
        if (___pawn is { Spawned: true } && ___pawn.TryGetComp<Comp_NightVision>() is { } comp)
        {
            comp.CheckAndAddHediff(hediff, part);
        }
    }
}