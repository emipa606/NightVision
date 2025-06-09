// Nightvision Harmony_Patches.cs
// 
// 30 03 2018
// 
// 21 07 2018

#if HARM12
using Harmony;
#else
using HarmonyLib;
#endif
using NightVision.Harmony.Manual;
using RimWorld;
using Verse;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MissingAnnotation
// ReSharper disable RegionWithSingleElement
// ReSharper disable All

// ReSharper disable InconsistentNaming

namespace NightVision;
#if DEBUG
            [HarmonyDebug]
#endif
[StaticConstructorOnStartup]
public static class NVHarmonyPatcher
{
    public static readonly HarmonyLib.Harmony NVHarmony;

    static NVHarmonyPatcher()
    {
        NVHarmony = new HarmonyLib.Harmony("drumad.rimworld.nightvision");

        var addHediffMethod = AccessTools.Method(
            typeof(Pawn_HealthTracker),
            nameof(Pawn_HealthTracker.AddHediff),
            [
                typeof(Hediff),
                typeof(BodyPartRecord),
                typeof(DamageInfo),
                typeof(DamageWorker.DamageResult)
            ]
        );

        var tryDropMethod = AccessTools.Method(
            typeof(Pawn_ApparelTracker),
            nameof(Pawn_ApparelTracker.TryDrop),
            [
                typeof(Apparel),
                typeof(Apparel).MakeByRefType(),
                typeof(IntVec3),
                typeof(bool)
            ]
        );

        NVHarmony.Patch(
            addHediffMethod,
            null,
            new HarmonyMethod(typeof(PawnHealthTracker_AddHediff),
                nameof(PawnHealthTracker_AddHediff.AddHediff_Postfix))
        );

        NVHarmony.Patch(
            tryDropMethod,
            null,
            new HarmonyMethod(typeof(ApparelTracker_TryDrop), nameof(ApparelTracker_TryDrop.Postfix))
        );


        NVHarmony.PatchAll();
    }
}