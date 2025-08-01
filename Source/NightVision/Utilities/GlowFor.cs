﻿// Nightvision Glow.cs
// 
// 17 10 2018
// 
// 17 10 2018

using Verse;

namespace NightVision;

/// <summary>
///     Helper methods related to pawn glow factors and nightvision comps
/// </summary>
public static class GlowFor
{
    // cache last value as repeated requests are common
    private static int cachedPawnHash;
    private static Comp_NightVision cachedComp;

    /// <summary>
    ///     Gets the glow at the given thing's position or a trivial fallback
    /// </summary>
    /// <returns>glow as float [0, 1]</returns>
    public static float GlowAt(Thing thing)
    {
        return thing?.Map?.glowGrid.GroundGlowAt(thing.Position) ?? Constants.TRIVIAL_GLOW;
    }

    /// <summary>
    ///     Gets the glow at the given position on the given map or a trivial fallback
    /// </summary>
    /// <returns>glow as float [0, 1]</returns>
    public static float GlowAt(Map map, IntVec3 pos)
    {
        return map?.glowGrid.GroundGlowAt(pos) ?? Constants.TRIVIAL_GLOW;
    }

    /// <summary>
    ///     Calculates the factor from glow for the given pawn at its current position
    /// </summary>
    /// <returns>multiplier, capped by settings</returns>
    public static float FactorOrFallBack(Pawn pawn)
    {
        if (CompFor(pawn) is { } comp && pawn?.Spawned == true)
        {
            return comp.FactorFromGlow(GlowAt(pawn));
        }

        return Constants.TRIVIAL_FACTOR;
    }

    /// <summary>
    ///     Calculates the factor from glow for the given pawn with the given glow
    /// </summary>
    /// <returns>multiplier, capped by settings</returns>
    public static float FactorOrFallBack(Pawn pawn, float glow)
    {
        if (CompFor(pawn) is { } comp)
        {
            return comp.FactorFromGlow(glow);
        }

        return Constants.TRIVIAL_FACTOR;
    }

    /// <summary>
    ///     Tries to find the nightvision comp of the given pawn.
    ///     Returns null if the pawn is null or pawn does not have comp.
    /// </summary>
    /// <param name="pawn"></param>
    /// <returns>the pawn's comp or NULL</returns>
    public static Comp_NightVision CompFor(Pawn pawn)
    {
        if (pawn == null)
        {
            return null;
        }

        if (pawn.GetHashCode() == cachedPawnHash)
        {
            return cachedComp;
        }

        if (pawn.TryGetComp<Comp_NightVision>() is not { } comp)
        {
            return null;
        }

        cachedComp = comp;
        cachedPawnHash = pawn.GetHashCode();

        return comp;
    }
}