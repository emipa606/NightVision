// Nightvision NVStatWorker_LightMultiplier.cs
// 
// 24 10 2018
// 
// 24 10 2018

using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace NightVision;

[UsedImplicitly]
public class NVStatWorker_LightMultiplier : NVStatWorker
{
    private static ApparelFlags GetEffectMaskForGlow(float glow)
    {
        if (glow.GlowIsBright())
        {
            return ApparelFlags.NullifiesPS;
        }

        return glow.GlowIsDarkness() ? ApparelFlags.GrantsNV : ApparelFlags.None;
    }


    public override string GetExplanationUnfinalized(
        StatRequest req,
        ToStringNumberSense numberSense
    )
    {
        if (req.Thing is not Pawn pawn
            || pawn.TryGetComp<Comp_NightVision>() is not { } comp ||
            ModLister.BiotechInstalled && pawn.genes.HasActiveGene(DarkVision))
        {
            return string.Empty;
        }

        var glow = GlowFor.GlowAt(pawn);
        return StatReportFor_NightVision.CompleteStatReport(Stat, GetEffectMaskForGlow(glow), comp, glow);
    }

    public override float GetValueUnfinalized(
        StatRequest req,
        bool applyPostProcess = true
    )
    {
        if (req.Thing is not Pawn pawn || ModLister.BiotechInstalled && pawn.genes.HasActiveGene(DarkVision))
        {
            return 1f;
        }

        if (pawn.TryGetComp<Comp_NightVision>() is not { } comp)
        {
            return 1f;
        }

        var glow = GlowFor.GlowAt(pawn);
        return comp.FactorFromGlow(glow);
    }

    public override bool IsDisabledFor(
        Thing thing
    )
    {
        return thing is not Pawn pawn || ModLister.BiotechInstalled && pawn.genes.HasActiveGene(DarkVision);
    }

    public override bool ShouldShowFor(StatRequest req)
    {
        return req.HasThing && !IsDisabledFor(req.Thing);
    }
}