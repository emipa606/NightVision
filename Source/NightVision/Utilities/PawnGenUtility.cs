using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace NightVision;

[NVHasSettingsDependentField]
public static class PawnGenUtility
{
    [NVSettingsDependentField] public static TriBool AnyPSHediffsExist = TriBool.Undefined;

    private static readonly List<string> ManuallyDisallowedHediffs = ["ArchotechEye"];


    private static List<Hediff_LightModifiers> PSHediffLightMods;


    [NVSettingsDependentField] public static Dictionary<string, PawnKindDef> cachedConvertedPawnKindDefs;

    [CanBeNull]
    public static HediffDef GetRandomPhotosensitiveHediffDef()
    {
        if (AnyPSHediffsExist.IsUndefined())
        {
            var query =
                from entry in Settings.Store.HediffLightMods
                where Classifier.ClassifyModifier(entry.Value[0], true) == VisionType.NVPhotosensitivity
                      && entry.Value.AffectsEye
                      && !ManuallyDisallowedHediffs.Contains(entry.Key.defName)
                select entry.Value;
            PSHediffLightMods = query.ToList();

            if (PSHediffLightMods.Count == 0)
            {
                AnyPSHediffsExist.MakeFalse();
            }
            else
            {
                AnyPSHediffsExist.MakeTrue();
            }
        }

        if (AnyPSHediffsExist.IsFalse())
        {
            return null;
        }

        return PSHediffLightMods.RandomElementByWeight(lm => Math.Max(lm[0] * 20, 1)).ParentDef as HediffDef;
    }

    private static void IterateFields(object source, object target, Action<Traverse, Traverse> action)
    {
        var sourceTrv = Traverse.Create(source);
        var targetTrv = Traverse.Create(target);
        AccessTools.GetFieldNames(source).ForEach(f =>
        {
            if (AccessTools.Field(source.GetType(), f) is { IsLiteral: true, IsInitOnly: false })
            {
            }
            else
            {
                action(sourceTrv.Field(f), targetTrv.Field(f));
            }
        });
    }

    [NotNull]
    public static PawnKindDef ConvertDefAndStoreOld([NotNull] PawnKindDef original)
    {
        cachedConvertedPawnKindDefs ??= new Dictionary<string, PawnKindDef>();

        if (cachedConvertedPawnKindDefs.TryGetValue(original.defName, out var storedPKD))
        {
            return storedPKD;
        }

        //var result = AccessTools.MakeDeepCopy<PawnKindDef>(original);

        //var result = new PawnKindDef();
        //Traverse.IterateFields(source: original, target: result, action: Traverse.CopyFields);

        var result = new PawnKindDef();

        IterateFields(original, result, Traverse.CopyFields);

        result.weaponTags.RemoveAll(str => !str.Contains("Melee") && !str.Contains("melee"));

        if (result.weaponTags.Count == 0)
        {
            Log.Message($"Error converting PawnKindDef for {original.defName}. No melee weapon tags. Using original");

            cachedConvertedPawnKindDefs[original.defName] = original;
        }

        result.apparelColor = new Color(Rand.Range(0.33f, 0.43f), Rand.Range(0.33f, 0.43f), Rand.Range(0.33f, 0.43f));
        result.apparelMoney.max *= 1.2f;
        result.apparelMoney.min *= 1.1f;
        result.itemQuality = QualityCategory.Normal;
        result.gearHealthRange = new FloatRange(0.8f, 1.4f);
        result.fleeHealthThresholdRange = FloatRange.Zero;

        cachedConvertedPawnKindDefs[original.defName] = result;

        return result;
    }
}