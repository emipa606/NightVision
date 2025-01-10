// Nightvision CompProperties_NightVision.cs
// 
// 16 05 2018
// 
// 21 07 2018

using System;
using Verse;

namespace NightVision;

public class CompProperties_NightVision : CompProperties
{
    public readonly bool CanCheat = false;
    public readonly float FullLightMultiplier = Constants.DEFAULT_FULL_LIGHT_MULTIPLIER;
    public readonly bool NaturalNightVision = false;
    public readonly bool NaturalPhotosensitivity = false;
    public readonly bool ShouldShowInSettings = true;

    public readonly float ZeroLightMultiplier = Constants.DEFAULT_ZERO_LIGHT_MULTIPLIER;

    public CompProperties_NightVision()
    {
        compClass = typeof(Comp_NightVision);
    }

    public bool IsDefault()
    {
        return Math.Abs(ZeroLightMultiplier - Constants.DEFAULT_ZERO_LIGHT_MULTIPLIER) < 0.1
               && Math.Abs(FullLightMultiplier - Constants.DEFAULT_FULL_LIGHT_MULTIPLIER) < 0.1
               && !(NaturalNightVision || NaturalPhotosensitivity);
    }
}