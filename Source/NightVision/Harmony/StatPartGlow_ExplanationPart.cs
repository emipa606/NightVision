using HarmonyLib;
using RimWorld;
using Verse;

namespace NightVision.Harmony;

[HarmonyPatch(typeof(StatPart_Glow), nameof(StatPart_Glow.ExplanationPart))]
public static class StatPartGlow_ExplanationPart
{
    public static void Postfix(
        ref StatRequest req,
        ref string __result
    )
    {
        if (__result.NullOrEmpty()
            || req.Thing is not Pawn pawn
            || pawn.TryGetComp<Comp_NightVision>() is not { } comp
            || ModLister.BiotechInstalled && pawn.genes.HasActiveGene(NVStatWorker.DarkVision))
        {
            return;
        }

        var glowat = pawn.Map.glowGrid.GroundGlowAt(pawn.Position);

        if (!(glowat < 0.3f) && !(glowat > 0.7f))
        {
            return;
        }

        __result = "StatsReport_LightMultiplier".Translate(glowat.ToStringPercent())
                   + ": ";

        __result += StatReportFor_NightVision.ShortStatReport(glowat, comp);
    }
}