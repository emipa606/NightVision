﻿// Nightvision NightVision HediffCompProperties_NightVision.cs
// 
// 16 05 2018
// 
// 21 07 2018

using System;
using JetBrains.Annotations;
using Verse;

namespace NightVision
{
    public class HediffCompProperties_NightVision : HediffCompProperties
    {
        public float                 FullLightMod           = default;
        public bool                  GrantsNightVision      = false;
        public bool                  GrantsPhotosensitivity = false;
        public Hediff_LightModifiers LightModifiers;
        public float                 ZeroLightMod = default;


        [UsedImplicitly]
        public HediffCompProperties_NightVision() => compClass = typeof(HediffComp_NightVision);

        public bool IsDefault()
            => Math.Abs(ZeroLightMod)    < 0.001f
               && Math.Abs(FullLightMod) < 0.001f
               && !(GrantsNightVision || GrantsPhotosensitivity);
    }
}