using RimWorld;
using Verse;

namespace NightVision;

public class NVStatWorker : StatWorker
{
    protected float Glow;

    protected ApparelFlags StatEffectMask;

    protected float DefaultStatValue;

    public string Acronym;

    protected StatDef Stat => stat;

    public static GeneDef DarkVision => DefDatabase<GeneDef>.GetNamedSilentFail("DarkVision");


    public override string GetExplanationUnfinalized(
        StatRequest req,
        ToStringNumberSense numberSense
    )
    {
        if (req.Thing is Pawn pawn && pawn.TryGetComp<Comp_NightVision>() is { } comp &&
            (!ModLister.BiotechInstalled || pawn.genes == null || !pawn.genes.HasActiveGene(DarkVision)))
        {
            return StatReportFor_NightVision.CompleteStatReport(Stat, StatEffectMask, comp, Glow);
        }

        return string.Empty;
    }

#if RW10
        public override string GetStatDrawEntryLabel(StatDef pStat, float value, ToStringNumberSense numberSense, StatRequest optionalReq)
        {
            return $"x{GetValueUnfinalized(optionalReq).ToStringPercent()}";
        }
#else
    public override string GetStatDrawEntryLabel(StatDef pStat, float value, ToStringNumberSense numberSense,
        StatRequest optionalReq,
        bool finalized = true)
    {
        return $"x{GetValueUnfinalized(optionalReq).ToStringPercent()}";
    }
#endif


    public override float GetValueUnfinalized(
        StatRequest req,
        bool applyPostProcess = true
    )
    {
        if (req.Thing is Pawn pawn && pawn.TryGetComp<Comp_NightVision>() is { } comp &&
            (!ModLister.BiotechInstalled || pawn.genes == null || !pawn.genes.HasActiveGene(DarkVision)))
        {
            return comp.FactorFromGlow(Glow);
        }

        return DefaultStatValue;
    }

    public override bool IsDisabledFor(Thing thing)
    {
        return thing is not Pawn pawn || pawn.TryGetComp<Comp_NightVision>() == null ||
               ModLister.BiotechInstalled && pawn.genes != null && pawn.genes.HasActiveGene(DarkVision);
    }

    public override bool ShouldShowFor(StatRequest req)
    {
        return req.HasThing && !IsDisabledFor(req.Thing);
    }

    public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
    {
        return "";
    }
}