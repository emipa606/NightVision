﻿// Nightvision CombatHelpers.cs
// 
// 26 09 2018
// 
// 20 10 2018

using System;
using UnityEngine;
using Verse;

namespace NightVision;

[NVHasSettingsDependentField]
public static class CombatHelpers
{
    [NVSettingsDependentField]
    private static float _attXCoeff = Settings.CombatStore.HitCurviness.Value / MultiplierCaps.Span;

    [NVSettingsDependentField] private static float _rangedCooldownMultiplierBad;
    [NVSettingsDependentField] private static float _rangedCooldownMultiplierGood;

    [NVSettingsDependentField]
    private static float _dodgeXCoeff = Settings.CombatStore.DodgeCurviness.Value / MultiplierCaps.Span;

    [NVSettingsDependentField]
    private static float _chanceOfSurpriseAttFactor = Settings.CombatStore.SurpriseAttackMultiplier.Value;

    private static FloatRange MultiplierCaps => Settings.Store.MultiplierCaps;

    public static float DodgeXCoeff
    {
        get
        {
            if (_dodgeXCoeff < 0)
            {
                _dodgeXCoeff = Settings.CombatStore.DodgeCurviness.Value / MultiplierCaps.Span;
            }

            return _dodgeXCoeff;
        }
        set => _dodgeXCoeff = value;
    }

    public static float AttXCoeff
    {
        get
        {
            if (!(_attXCoeff < -1))
            {
                return _attXCoeff;
            }

            _attXCoeff = Settings.CombatStore.HitCurviness.Value / MultiplierCaps.Span;

            return _attXCoeff;
        }
        set => _attXCoeff = value;
    }

    public static float ChanceOfSurpriseAttFactor
    {
        get
        {
            if (_chanceOfSurpriseAttFactor < -1)
            {
                _chanceOfSurpriseAttFactor = Settings.CombatStore.SurpriseAttackMultiplier.Value;
            }

            return _chanceOfSurpriseAttFactor;
        }
        set => _chanceOfSurpriseAttFactor = value;
    }

    public static float RangedCooldownMultiplierBad
    {
        get
        {
            if (!(_rangedCooldownMultiplierBad < -1))
            {
                return _rangedCooldownMultiplierBad;
            }

            if (Settings.CombatStore.RangedCooldownLinkedToCaps.Value)
            {
                _rangedCooldownMultiplierBad = 1 / MultiplierCaps.min;
            }
            else
            {
                _rangedCooldownMultiplierBad = Settings.CombatStore.RangedCooldownMinAndMax.Value.max / 100f;
            }

            return _rangedCooldownMultiplierBad;
        }
        set => _rangedCooldownMultiplierBad = value;
    }

    private static float RangedCooldownMultiplierGood
    {
        get
        {
            if (!(_rangedCooldownMultiplierGood < -1))
            {
                return _rangedCooldownMultiplierGood;
            }

            if (Settings.CombatStore.RangedCooldownLinkedToCaps.Value)
            {
                _rangedCooldownMultiplierGood = 1 / MultiplierCaps.max;
            }
            else
            {
                _rangedCooldownMultiplierGood = Settings.CombatStore.RangedCooldownMinAndMax.Value.min / 100f;
            }

            return _rangedCooldownMultiplierGood;
        }
    }

    public static float HitChanceGlowTransform(float hitChance, float attGlowFactor)
    {
        if (hitChance + 0.001f > 1)
        {
            return hitChance;
        }

        return 1 / (1 + (((1 / hitChance) - 1) * (float)Math.Exp(-1 * AttXCoeff * (attGlowFactor - 1))));
    }

    public static string NightVisionTooltipElement(Thing target)
    {
        var result = "";
        // 16.08.2021 RW1.3 replaced .forcedMissRadius with public property
        if (CurrentShot.NoShot || CurrentShot.Verb.verbProps.ForcedMissRadius > 0.5f ||
            CurrentShot.GlowFactor.FactorIsTrivial())
        {
            return result;
        }

        result += $"   {Str_Combat.AimFactorFromLight(GlowFor.GlowAt(target), CurrentShot.PseudoMultiplier())}";

        return result;
    }

    public static float AdjustCooldownForGlow(float rangedCooldown, Pawn pawn)
    {
        if (pawn == null)
        {
            return rangedCooldown;
        }

        if (pawn.TryGetComp<Comp_NightVision>() is not { } comp)
        {
            return rangedCooldown;
        }

        var glowGrid = pawn.Map?.glowGrid;

        if (glowGrid == null)
        {
            return rangedCooldown;
        }

        var glow = glowGrid.GroundGlowAt(pawn.Position);

        if (!glow.GlowIsDarkOrBright())
        {
            return rangedCooldown;
        }

        var glF = comp.FactorFromGlow(glow);

        var skillLevel = pawn.skills.GetSkill(Defs_Rimworld.ShootSkill).Level;

        return rangedCooldown * RangedCooldownMultiplier(skillLevel, glF);
    }

    public static float GlowFactorForPawnAtTarget(Pawn pawn, LocalTargetInfo target, Comp_NightVision comp)
    {
        return comp.FactorFromGlow(GlowFor.GlowAt(pawn.Map, target.Cell));
    }


    /// <summary>
    ///     if glow factor is &lt; 1f then result is &gt; 1f and tends towards 1f as skill tends towards 20.
    ///     if glow factor is &gt; 1f then result is &lt; 1f and tends towards reciprocal of max glow factor as skill tends
    ///     towards 20.
    /// </summary>
    /// <param name="skill"></param>
    /// <param name="glowFactor"></param>
    /// <returns></returns>
    public static float RangedCooldownMultiplier(int skill, float glowFactor)
    {
        if (glowFactor < 1f - Constants.NV_EPSILON)
        {
            return 1 + ((1 - glowFactor) * RangedCooldownMultiplierBad * (1 - (float)Math.Sqrt(0.05f * skill)));
        }

        if (glowFactor > 1f + Constants.NV_EPSILON)
        {
            return 1 + ((1 - glowFactor) * RangedCooldownMultiplierGood * (float)Math.Sqrt(0.05f * skill));
        }

        return 1;
    }

    public static float SurpriseAttackChance(float atkGlowFactor, float defGlowFactor)
    {
        return SurpriseAttackChance(atkGlowFactor - defGlowFactor);
    }

    /// <summary>
    ///     Surprise attack chance can never be negative
    /// </summary>
    /// <param name="glowFactorDelta">attacker's - defender's</param>
    /// <returns></returns>
    public static float SurpriseAttackChance(float glowFactorDelta)
    {
        return Mathf.Clamp01(glowFactorDelta) * ChanceOfSurpriseAttFactor;
    }


    /// <param name="orgDodge">defenders dodge chance</param>
    /// <param name="glowFactorDelta">AttGlowFactor - DefGlowFactor</param>
    /// <returns></returns>
    public static float DodgeChanceFunction(float orgDodge, float glowFactorDelta)
    {
        if (glowFactorDelta.IsTrivial())
        {
            return orgDodge;
        }

        return 2 * orgDodge / (1 + (float)Math.Exp(DodgeXCoeff * glowFactorDelta));
    }
}