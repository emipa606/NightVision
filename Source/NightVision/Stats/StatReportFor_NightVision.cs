// Nightvision StatReportFor_NightVision.cs
// 
// 25 10 2018
// 
// 06 12 2018

using System;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace NightVision;

public static class StatReportFor_NightVision
{
    public static string CompleteStatReport(StatDef stat, ApparelFlags effectMask, Comp_NightVision comp,
        float relevantGlow)
    {
        var factorFromGlow = comp.FactorFromGlow(relevantGlow);

        return BasicExplanation(relevantGlow, out var UsedApparel, comp)
               + FinalValue(stat, factorFromGlow)
               + (effectMask != ApparelFlags.None && UsedApparel ? ApparelPart(effectMask, comp) : "");
    }

    public static string ShortStatReport(float glow, Comp_NightVision comp)
    {
        return BasicExplanation(glow, out _, comp, true);
    }


    private static string ApparelPart(ApparelFlags effectMask, Comp_NightVision comp)
    {
        var builder = new StringBuilder();
        builder.AppendLine("StatsReport_RelevantGear".Translate());
        var nvApparel = Settings.Store.NVApparel;
        foreach (var app in comp.PawnsNVApparel ?? Enumerable.Empty<Apparel>())
        {
            if (nvApparel.TryGetValue(app.def, out var setting)
                && setting.HasEffect(effectMask))
            {
                builder.AppendLine(app.LabelCap);
            }
        }

        return builder.ToString();
    }


    /// <summary>
    ///     For the pawn's stat inspect tab. Cleaned up a bit, still about as elegant as a panda doing the can-can
    /// </summary>
    /// <param name="glow"></param>
    /// <param name="usedApparelSetting">if apparel had an effect</param>
    /// <param name="comp"></param>
    /// <param name="needsFinalValue">if final value is added externally, or we need to add it</param>
    /// <returns></returns>
    private static string BasicExplanation(float glow, out bool usedApparelSetting, Comp_NightVision comp,
        bool needsFinalValue = false)
    {
        var nvsum = 0f;
        var pssum = 0f;
        var sum = 0f;
        var caps = LightModifiersBase.GetCapsAtGlow(glow);
        var foundSomething = false;
        float effect;
        float basevalue;
        var lowLight = glow.GlowIsDarkness();
        usedApparelSetting = false;


        var explanation = new StringBuilder();

        var nvexplanation = new StringBuilder().AppendFormat(
            Str.ExpIntro,
            "",
            Str.MaxAtGlow(glow),
            caps[2],
            Str.NightVision
        ).AppendLine();

        var psexplanation = new StringBuilder().AppendFormat(
            Str.ExpIntro,
            "",
            Str.MaxAtGlow(glow),
            caps[3],
            Str.Photosens
        ).AppendLine();


        explanation.AppendLine();

        if (lowLight)
        {
            basevalue = Constants.DEFAULT_FULL_LIGHT_MULTIPLIER
                        + ((Constants.DEFAULT_ZERO_LIGHT_MULTIPLIER - Constants.DEFAULT_FULL_LIGHT_MULTIPLIER)
                           * (0.3f - glow)
                           / 0.3f);

            if (comp.ApparelGrantsNV)
            {
                foundSomething = true;
            }
        }
        else
        {
            basevalue = Constants.DEFAULT_FULL_LIGHT_MULTIPLIER;

            if (glow.GlowIsBright() && comp.ApparelNullsPS)
            {
                foundSomething = true;
            }
        }

        explanation.AppendFormat("  " + Str.MultiplierLine, "StatsReport_BaseValue".Translate(), basevalue).AppendLine()
            .AppendLine();

        string StringToAppend;

        if (comp.NaturalLightModifiers.HasAnyModifier() && comp.NumberOfRemainingEyes > 0)
        {
            effect = comp.NaturalLightModifiers.GetEffectAtGlow(glow);

            if (effect.IsNonTrivial())
            {
                foundSomething = true;

                var NumToAdd = (float)Math.Round(
                    effect * comp.NumberOfRemainingEyes,
                    Constants.NUMBER_OF_DIGITS,
                    Constants.ROUNDING
                );

                StringToAppend = string.Format(
                    "    " + Str.ModifierLine,
                    $"{comp.ParentPawn.def.LabelCap} {comp.RaceSightParts.First().LabelShort} x{comp.NumberOfRemainingEyes}",
                    effect * comp.NumberOfRemainingEyes
                );

                switch (comp.NaturalLightModifiers.Setting)
                {
                    case VisionType.NVNightVision:
                        nvsum += NumToAdd;
                        nvexplanation.AppendLine(StringToAppend);

                        break;
                    case VisionType.NVPhotosensitivity:
                        pssum += NumToAdd;
                        psexplanation.AppendLine(StringToAppend);

                        break;
                    case VisionType.NVCustom:
                        sum += NumToAdd;
                        explanation.AppendLine(StringToAppend);

                        break;
                }
            }
        }

        foreach (var value in comp.PawnsNVHediffs.Values)
        {
            if (value.NullOrEmpty())
            {
                continue;
            }


            var hediffLightMods = Settings.Store.HediffLightMods;
            foreach (var hediffDef in value)
            {
                if (!hediffLightMods.TryGetValue(hediffDef, out var hediffSetting))
                {
                    continue;
                }

                effect = hediffSetting.GetEffectAtGlow(glow, comp.EyeCount);

                if (effect.IsNonTrivial())
                {
                    foundSomething = true;

                    effect = (float)Math.Round(
                        effect,
                        Constants.NUMBER_OF_DIGITS,
                        Constants.ROUNDING
                    );

                    StringToAppend = string.Format("    " + Str.ModifierLine, hediffDef.LabelCap, effect);

                    switch (hediffSetting.IntSetting)
                    {
                        case VisionType.NVNightVision:
                            nvsum += effect;
                            nvexplanation.AppendLine(StringToAppend);

                            break;
                        case VisionType.NVPhotosensitivity:
                            pssum += effect;
                            psexplanation.AppendLine(StringToAppend);

                            break;
                        case VisionType.NVCustom:
                            sum += effect;
                            explanation.AppendLine(StringToAppend);

                            break;
                    }
                }
            }
        }

        if (!foundSomething)
        {
            return needsFinalValue ? comp.FactorFromGlow(glow).ToStringPercent() : string.Empty;
        }

        if (nvsum.IsNonTrivial())
        {
            explanation.Append(nvexplanation);
            explanation.AppendLine();
        }

        if (pssum.IsNonTrivial())
        {
            explanation.Append(psexplanation);
            explanation.AppendLine();
        }

        sum += pssum + nvsum;

        explanation.AppendFormat(Str.ModifierLine, "NVTotal".Translate() + " " + "NVModifier".Translate(), sum);

        explanation.AppendLine();


        var needed = true;

        if (!comp.CanCheat)
        {
            if (sum - Constants.NV_EPSILON > caps[0] || sum + Constants.NV_EPSILON < caps[1])
            {
                AppendPreSumIfNeeded(ref needed);

                explanation.AppendFormat(
                    Str.Maxline,
                    "NVTotal".Translate() + " ",
                    "max".Translate(),
                    sum > caps[0] ? caps[0] : caps[1]
                );

                explanation.AppendLine();
            }

            if (lowLight && comp.ApparelGrantsNV && sum + Constants.NV_EPSILON < caps[2])
            {
                AppendPreSumIfNeeded(ref needed);
                explanation.Append("NVGearPresent".Translate($"{basevalue + caps[2]:0%}"));
                usedApparelSetting = true;
                sum = caps[2];
            }
            else if (comp.ApparelNullsPS && sum + Constants.NV_EPSILON < 0)
            {
                AppendPreSumIfNeeded(ref needed);
                explanation.Append("PSGearPresent".Translate($"{Constants.DEFAULT_FULL_LIGHT_MULTIPLIER:0%}"));
                usedApparelSetting = true;
                sum = 0;
            }
        }

        explanation.AppendLine();

        if (!needsFinalValue)
        {
            return explanation.ToString();
        }

        sum += basevalue;

        explanation.AppendFormat(
            Str.MultiplierLine,
            "NVStatReport_FinalMulti".Translate(),
            sum > caps[0] + basevalue ? caps[0] + basevalue :
            sum < caps[1] + basevalue ? caps[1] + basevalue : sum
        );

        return explanation.ToString();

        //Fallback 

        void AppendPreSumIfNeeded(ref bool isNeeded)
        {
            if (!isNeeded)
            {
                return;
            }

            explanation.AppendFormat(
                Str.MultiplierLine,
                "NVTotal".Translate() + " " + "NVMultiplier".Translate(),
                sum + basevalue
            );

            explanation.AppendLine();

            isNeeded = false;
        }
    }

    private static string FinalValue(StatDef stat, float value)
    {
        return "StatsReport_FinalValue".Translate() + ": " + stat.ValueToString(value, stat.toStringNumberSense) +
               "\n\n";
    }
}