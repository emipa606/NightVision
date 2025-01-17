﻿using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace NightVision.Harmony;

[HarmonyPatch(typeof(HediffDef), nameof(HediffDef.SpecialDisplayStats))]
public static class HediffDef_SpecialDisplayStats
{
    public static IEnumerable<StatDrawEntry> Postfix(
        IEnumerable<StatDrawEntry> statdrawentries,
        HediffDef __instance
    )
    {
        var statDrawEntryList = statdrawentries.ToList();

        foreach (var sDE in statDrawEntryList)
        {
            yield return sDE;
        }

        if (!Settings.Store.HediffLightMods.TryGetValue(__instance, out var hlm)
            || !hlm.HasAnyModifier())
        {
            yield break;
        }

        var vt = hlm.Setting;

        if (vt < VisionType.NVCustom)
        {
            yield return new StatDrawEntry(
                Defs_Rimworld.BasicStats,
                "NVGrantsVisionType".Translate().RawText,
                vt.ToString().Translate().RawText,
                hlm.AffectsEye ? "NVHediffQualifier".Translate().RawText : "",
                0
            );
        }
        else
        {
            yield return new StatDrawEntry(
                Defs_Rimworld.BasicStats,
                "NVGrantsVisionType".Translate().RawText,
                vt.ToString(),
                hlm.AffectsEye ? "NVHediffQualifier".Translate().RawText : "",
                0
            );

            yield return new StatDrawEntry(
                Defs_Rimworld.BasicStats,
                Defs_NightVision.NightVision,
                hlm[0],
                StatRequest.ForEmpty(),
                ToStringNumberSense.Offset
            );

            yield return new StatDrawEntry(
                Defs_Rimworld.BasicStats,
                Defs_NightVision.LightSensitivity,
                hlm[1],
                StatRequest.ForEmpty(),
                ToStringNumberSense.Offset
            );
        }
    }
}