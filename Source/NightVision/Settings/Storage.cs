﻿// Nightvision Storage.cs
// 
// 03 08 2018
// 
// 24 10 2018

using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace NightVision;

public class Storage
{
    public const float HighestCap = 2f;

    public const float LowestCap = 0.01f;
    public HashSet<ThingDef> AllEyeCoveringHeadgearDefs = [];

    //AllEyeHediffs is a subset of AllSightAffectingHediffs
    public HashSet<HediffDef> AllEyeHediffs = [];
    public HashSet<HediffDef> AllSightAffectingHediffs = [];
    public bool CustomCapsEnabled;

    public Dictionary<HediffDef, Hediff_LightModifiers> HediffLightMods = new();

    public FloatRange MultiplierCaps = new(
        Constants.DEFAULT_MIN_CAP,
        Constants.DEFAULT_MAX_CAP
    );

    public bool NullRefWhenLoading;

    public Dictionary<ThingDef, ApparelVisionSetting> NVApparel = new();

    private bool NVEnabledForCE = true;


    public Dictionary<ThingDef, Race_LightModifiers>
        RaceLightMods = new();

    /// <summary>
    ///     Loads/Saves mod settings (except those relating to combat which have a separate manager)
    /// </summary>
    public void ExposeSettings()
    {
        // Checks if the flag for custom multiplier caps has been enabled - default is false
        Scribe_Values.Look(ref CustomCapsEnabled, "CustomLimitsEnabled");
        // If custom caps are enabled then scribe them
        if (CustomCapsEnabled)
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                if (MultiplierCaps.min > MultiplierCaps.max)
                {
                    (MultiplierCaps.max, MultiplierCaps.min) = (MultiplierCaps.min, MultiplierCaps.max);
                }
            }

            Scribe_Values.Look(ref MultiplierCaps.min, "LowerLimit", 0.8f);
            Scribe_Values.Look(ref MultiplierCaps.max, "UpperLimit", 1.2f);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (MultiplierCaps.min > MultiplierCaps.max)
                {
                    (MultiplierCaps.max, MultiplierCaps.min) = (MultiplierCaps.min, MultiplierCaps.max);
                }
            }
        }

        // Checks whether the flare raid and combat extended changes have been enabled
        Scribe_Values.Look(ref NVGameComponent.FlareRaidIsEnabled, "flareRaidEnabled");
        Scribe_Values.Look(ref NVEnabledForCE, "EnabledForCombatExtended", true);

        // Scribes the standard modifiers for PS & NV. Uses Scribe_Deep as light modifiers are a custom class
        //cctor args because otherwise statics don't seem to load properly
        Scribe_Deep.Look(ref LightModifiersBase.PSLightModifiers, "photosensitivitymodifiers", true,
            false);

        Scribe_Deep.Look(ref LightModifiersBase.NVLightModifiers, "nightvisionmodifiers", false,
            true);

        // If we are loading settings and there are no saved settings for PS/NV (either because 1st time loading
        // NVMod or because settings were left as default which we don't save) then we load the default settings
        if (Scribe.mode == LoadSaveMode.LoadingVars)
        {
            LightModifiersBase.PSLightModifiers ??= new LightModifiersBase
            {
                Offsets = Constants.PSDefaultOffsets.ToArray(),
                Initialised = true
            };

            LightModifiersBase.NVLightModifiers ??= new LightModifiersBase
            {
                Offsets = Constants.NVDefaultOffsets.ToArray(),
                Initialised = true
            };
        }

        // We scribe the Race, Hediff, and Apparel settings using custom scribes. The methods return true if there 
        // was an entry in mod's settings that referred to a def that no longer exists (generally because a mod
        // was removed).
        var nullRef = Scribes.LightModifiersDict(ref RaceLightMods, "Races");
        nullRef |= Scribes.LightModifiersDict(ref HediffLightMods, "Hediffs");
        nullRef |= Scribes.ApparelDict(ref NVApparel);
        // THis is used later to clean up the code
        NullRefWhenLoading = nullRef;
        //FieldClearer.ResetSettingsDependentFields();
    }

    public void ResetAllSettings()
    {
        Log.Message("NightVision: Defaulting Settings");

        CustomCapsEnabled = false;

        MultiplierCaps = new FloatRange(
            Constants.DEFAULT_MIN_CAP,
            Constants.DEFAULT_MAX_CAP
        );

        NVEnabledForCE = true;
        NVGameComponent.FlareRaidIsEnabled = true;
        LightModifiersBase.PSLightModifiers.Offsets = LightModifiersBase.PSLightModifiers.DefaultOffsets.ToArray();

        LightModifiersBase.NVLightModifiers.Offsets = LightModifiersBase.NVLightModifiers.DefaultOffsets.ToArray();

        Settings.CombatStore.LoadDefaultSettings();

        Log.Message("NightVision.Storage.ResetAllSettings: Clearing Dictionaries");
        RaceLightMods = null;
        HediffLightMods = null;
        NVApparel = null;
        AllEyeCoveringHeadgearDefs = null;
        AllEyeHediffs = null;
        AllSightAffectingHediffs = null;
        Log.Message("NightVision.Storage.ResetAllSettings: Rebuilding Dictionaries");
        var initialiser = new Initialiser();
        initialiser.FindDefsToAddNightVisionTo();

        Settings.Cache.Reset();
        FieldClearer.ResetSettingsDependentFields();
    }

    public void SetMinMultiplierCap(float newMin)
    {
        MultiplierCaps.min = (float)Math.Round(newMin / 100, Constants.NUMBER_OF_DIGITS);
    }

    public void SetMaxMultiplierCap(float newMax)
    {
        MultiplierCaps.max = (float)Math.Round(newMax / 100, Constants.NUMBER_OF_DIGITS);
    }

    public float ClampToMultipliers(float val)
    {
        if (val < MultiplierCaps.min - Constants.NV_EPSILON)
        {
            return MultiplierCaps.min;
        }

        return val > MultiplierCaps.max + Constants.NV_EPSILON ? MultiplierCaps.max : val;
    }
}