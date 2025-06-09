// Nightvision HediffCompProperties_NightVision.cs
// 
// 16 05 2018
// 
// 21 07 2018

using System;
using JetBrains.Annotations;
using Verse;

namespace NightVision;

public class HediffCompProperties_NightVision : HediffCompProperties
{
    public readonly float FullLightMod = 0;
    public readonly bool GrantsNightVision = false;
    public readonly bool GrantsPhotosensitivity = false;
    public readonly float ZeroLightMod = 0;
    public Hediff_LightModifiers LightModifiers;


    [UsedImplicitly]
    public HediffCompProperties_NightVision()
    {
        compClass = typeof(HediffComp_NightVision);
    }

    public bool IsDefault()
    {
        return Math.Abs(ZeroLightMod) < Constants.NV_EPSILON
               && Math.Abs(FullLightMod) < Constants.NV_EPSILON
               && !(GrantsNightVision || GrantsPhotosensitivity);
    }
}