// Nightvision NVStatWorker_RangedCooldown.cs
// 
// 17 10 2018
// 
// 17 10 2018

using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace NightVision;

[UsedImplicitly]
public class NVStatWorker_RangedCooldown : NVStatWorker
{
    public readonly SkillDef DerivedFrom = Defs_Rimworld.ShootSkill;

    public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
    {
        if (req.Thing is not Pawn pawn)
        {
            return string.Empty;
        }

        var skillLevel = pawn.skills.GetSkill(DerivedFrom).Level;

        return StatReportFor_NightVision_Combat.RangedCoolDown(pawn, skillLevel);
    }

    public override string GetStatDrawEntryLabel(StatDef statDef, float value, ToStringNumberSense numberSense,
        StatRequest optionalReq, bool finalized = true)
    {
        return $"x{GetValueUnfinalized(optionalReq).ToStringPercent()}";
    }

    public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
    {
        if (req.Thing is not Pawn pawn)
        {
            return Constants.TRIVIAL_FACTOR;
        }

        var glowFactor = GlowFor.FactorOrFallBack(pawn);

        return glowFactor.FactorIsNonTrivial()
            ? CombatHelpers.RangedCooldownMultiplier(pawn.skills.GetSkill(DerivedFrom).Level, glowFactor)
            : Constants.TRIVIAL_FACTOR;
    }

    public override bool IsDisabledFor(Thing thing)
    {
        return base.IsDisabledFor(thing) || !(thing is Pawn pawn && !pawn.skills.GetSkill(DerivedFrom).TotallyDisabled);
    }

    public override bool ShouldShowFor(StatRequest req)
    {
        return base.ShouldShowFor(req) || !Settings.CombatStore.RangedCooldownEffectsEnabled.Value;
    }
}