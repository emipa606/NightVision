using HarmonyLib;
using RimWorld;
using Verse;

namespace NightVision.Harmony;

[HarmonyPatch(typeof(StatPart_Glow), "FactorFromGlow")]
public static class StatPartGlow_FactorFromGlow
{
    public static void Postfix(Thing t, ref float __result)
    {
        if (t is not Pawn pawn || pawn.TryGetComp<Comp_NightVision>() is not { } comp)
        {
            return;
        }

        var glowat = pawn.Map.glowGrid.GroundGlowAt(pawn.Position);
        __result = comp.FactorFromGlow(glowat);
    }
}