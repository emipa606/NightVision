﻿using Verse;

namespace NVTesting
{
    public class AvoidGlow
        {
            public byte[] AvoidGlowGrid;

            public static void InitAvoidGlowGrid(
                Pawn pawn)
                {
                    GlowGrid localGlowGrid = pawn?.Map?.glowGrid;
                    if (localGlowGrid == null)
                        {
                            return;
                        }

                    
                }
        }
}
