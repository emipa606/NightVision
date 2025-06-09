using HarmonyLib;
using RimWorld;
using Verse;

namespace NightVision.Harmony;

[HarmonyPatch(typeof(ThoughtWorker_Dark), "CurrentStateInternal")]
public static class ThoughtWorkerDark_CurrentStateInternal
{
    private const int PhotosensDarkThoughtStage = 1;

    public static void Postfix(
        Pawn p,
        ref ThoughtState __result
    )
    {
        if (!__result.Active)
        {
            return;
        }

        if (p.TryGetComp<Comp_NightVision>() is not { } comp)
        {
            return;
        }

        switch (comp.PsychDark)
        {
            default:
                return;
            case VisionType.NVNightVision:
                __result = ThoughtState.Inactive;

                return;
            case VisionType.NVPhotosensitivity:

                __result = ThoughtState.ActiveAtStage(
                    PhotosensDarkThoughtStage,
                    nameof(VisionType.NVPhotosensitivity).Translate()
                );

                return;
        }
    }
}