﻿// Nightvision NightVision Glow.cs
// 
// 17 10 2018
// 
// 17 10 2018

using Verse;

namespace NightVision
{
    public static class GlowFor
    {
        public static float GlowAt(Thing thing)
        {
            
            return thing?.Map?.glowGrid.GameGlowAt(thing.Position) ?? Constants_Calculations.TrivialGlow;
        }

        public static float GlowAt(Map map, IntVec3 pos)
        {
            return map?.glowGrid.GameGlowAt(pos) ?? Constants_Calculations.TrivialGlow;
        }

        public static float FactorOrFallBack(Pawn pawn, float glow)
        {
            if (CompFor(pawn) is Comp_NightVision comp)
            {
                return comp.FactorFromGlow(glow);
            }

            return Constants_Calculations.TrivialFactor;
        }

        public static int cachedPawnHash;
        public static Comp_NightVision cachedComp;

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
            else if (pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                cachedComp = comp;
                cachedPawnHash = pawn.GetHashCode();

                return comp;
            }
            else
            {
                return null;
            }
        }

        public static float FactorOrFallBack(Pawn pawn)
        {
            if (CompFor(pawn) is Comp_NightVision comp && pawn?.Spawned == true)
            {
                return comp.FactorFromGlow(GlowAt(pawn));
            }

            return Constants_Calculations.TrivialFactor;
        }
    }
}