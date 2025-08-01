﻿// Nightvision Hediff_LightModifiers.cs
// 
// 16 05 2018
// 
// 21 07 2018

using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace NightVision;

public class Hediff_LightModifiers : LightModifiersBase
{
    private float[] _defaultOffsets;

    [CanBeNull] private HediffCompProperties_NightVision _hediffCompProps;

    private HediffDef _parentDef;
    public bool AffectsEye = false;
    public bool AutoAssigned = false;

    public VisionType IntSetting;

    public Hediff_LightModifiers()
    {
    }

    public Hediff_LightModifiers(
        HediffDef hediffDef
    )
    {
        _parentDef = hediffDef;
        AttachCompProps();
    }

    public override float this[
        int index
    ]
    {
        get
        {
            switch (IntSetting)
            {
                default:
                    return 0f;
                case VisionType.NVNightVision:
                    return NVLightModifiers[index];
                case VisionType.NVPhotosensitivity:
                    return PSLightModifiers[index];
                case VisionType.NVCustom:
                    return Offsets[index];
            }
        }
        set
            => Offsets[index] = (float)Math.Round(
                Mathf.Clamp(
                    value,
                    -0.99f + (0.2f * (1 - index)),
                    +1f + (0.2f * (1 - index))
                ),
                2,
                Constants.ROUNDING
            );
    }

    public override VisionType Setting
    {
        get => IntSetting;
        set => IntSetting = value;
    }

    public override Def ParentDef => _parentDef;

    public VisionType DefaultSetting => AutoAssigned
        ? AutoQualifier.HediffCheck(_parentDef) ?? VisionType.NVNone
        : GetSetting(_hediffCompProps);

    public override float[] DefaultOffsets
    {
        get
        {
            if (_defaultOffsets != null)
            {
                return _defaultOffsets;
            }

            switch (DefaultSetting)
            {
                default:
                    _defaultOffsets = new float[2];

                    break;
                case VisionType.NVNightVision:
                    _defaultOffsets = NVLightModifiers.Offsets.ToArray();

                    break;
                case VisionType.NVPhotosensitivity:
                    _defaultOffsets = PSLightModifiers.Offsets.ToArray();

                    break;
                case VisionType.NVCustom:

                    _defaultOffsets = _hediffCompProps != null
                        ? [_hediffCompProps.ZeroLightMod, _hediffCompProps.FullLightMod]
                        : new float[2];

                    break;
            }

            return _defaultOffsets;
        }
    }

    public override void ExposeData()
    {
        Scribe_Defs.Look(ref _parentDef, "HediffDef");
        Scribe_Values.Look(ref IntSetting, "Setting", forceSave: true);

        if (IntSetting == VisionType.NVCustom)
        {
            base.ExposeData();
        }

        if (Scribe.mode != LoadSaveMode.LoadingVars || ParentDef == null)
        {
            return;
        }

        Initialised = true;
        AttachCompProps();
    }

    /// <summary>
    /// </summary>
    /// <param name="glow"> [0,1]</param>
    /// <param name="numOfEyesNormalisedFor">Required for hediffs</param>
    /// <returns>Normalised value if hediff is an eye hediff</returns>
    public override float GetEffectAtGlow(
        float glow,
        int numOfEyesNormalisedFor = 1)
    {
        return AffectsEye ? base.GetEffectAtGlow(glow, numOfEyesNormalisedFor) : base.GetEffectAtGlow(glow);
    }

    public void InitialiseNewFromSettings(
        HediffDef hediffDef
    )
    {
        Initialised = true;
        _parentDef = hediffDef;
        AttachCompProps();
    }


    public override bool ShouldBeSaved()
    {
        if (AutoAssigned)
        {
            return IntSetting != AutoQualifier.HediffCheck(_parentDef);
        }

        if (_hediffCompProps == null)
        {
            return false;
        }

        switch (IntSetting)
        {
            default:
                return !_hediffCompProps.IsDefault();
            case VisionType.NVNightVision:
                return !_hediffCompProps.GrantsNightVision;
            case VisionType.NVPhotosensitivity:
                return !_hediffCompProps.GrantsPhotosensitivity;
            case VisionType.NVCustom:

                return !(Math.Abs(_hediffCompProps.FullLightMod - Offsets[1]) < Constants.NV_EPSILON)
                       || !(Math.Abs(_hediffCompProps.ZeroLightMod - Offsets[0]) < Constants.NV_EPSILON);
        }
    }

    public static VisionType GetCompSetting(
        Hediff_LightModifiers hLm
    )
    {
        return GetSetting(hLm._hediffCompProps);
    }


    private void AttachCompProps()
    {
        if (_parentDef.CompPropsFor(typeof(HediffComp_NightVision)) is HediffCompProperties_NightVision
            compProps)
        {
            _hediffCompProps = compProps;
            compProps.LightModifiers = this;

            if (Initialised)
            {
                return;
            }

            IntSetting = GetSetting(compProps);
            Offsets = [compProps.ZeroLightMod, compProps.FullLightMod];
        }
        else
        {
            _parentDef.comps ??= [];

            _hediffCompProps = new HediffCompProperties_NightVision { LightModifiers = this };
            _parentDef.comps.Add(_hediffCompProps);
            Initialised = true;
        }
    }

    private static VisionType GetSetting(
        HediffCompProperties_NightVision compprops
    )
    {
        if (compprops == null)
        {
            return VisionType.NVNone;
        }

        if (compprops.GrantsNightVision)
        {
            return VisionType.NVNightVision;
        }

        if (compprops.GrantsPhotosensitivity)
        {
            return VisionType.NVPhotosensitivity;
        }

        return compprops.IsDefault() ? VisionType.NVNone : VisionType.NVCustom;
    }
}