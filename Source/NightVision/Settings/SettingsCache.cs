// Nightvision SettingsCache.cs
// 
// 03 08 2018
// 
// 16 10 2018

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace NightVision;

public class SettingsCache
{
    [CanBeNull] private List<HediffDef> _allHediffsCache;

    [CanBeNull] private List<ThingDef> _headgearCache;

    public bool CacheInited;

    public float? MaxCache;

    public float? MinCache;
    public float? NVFullCache;
    public float? NVZeroCache;
    public float? PSFullCache;
    public float? PSZeroCache;

    [NotNull]
    public List<ThingDef> GetAllHeadgear
    {
        get
        {
            if (_headgearCache != null && _headgearCache.Count != 0)
            {
                return _headgearCache;
            }

            _headgearCache = [..Settings.Store.AllEyeCoveringHeadgearDefs];

            foreach (var appareldef in Settings.Store.NVApparel.Keys)
            {
                if (_headgearCache == null)
                {
                    continue;
                }

                var appindex = _headgearCache.IndexOf(appareldef);

                if (appindex <= 0)
                {
                    continue;
                }

                _headgearCache.RemoveAt(appindex);
                _headgearCache.Insert(0, appareldef);
            }

            _headgearCache = [.. _headgearCache.OrderBy(def => def.label)];
            return _headgearCache;
        }
    }

    [NotNull]
    public List<HediffDef> GetAllHediffs
    {
        get
        {
            if (_allHediffsCache != null && _allHediffsCache.Count != 0)
            {
                return _allHediffsCache;
            }

            _allHediffsCache = [..Settings.Store.AllSightAffectingHediffs];

            foreach (var hediffdef in Settings.Store.HediffLightMods.Keys)
            {
                if (_allHediffsCache == null)
                {
                    continue;
                }

                var appindex = _allHediffsCache.IndexOf(hediffdef);

                if (appindex <= 0)
                {
                    continue;
                }

                _allHediffsCache.RemoveAt(appindex);
                _allHediffsCache.Insert(0, hediffdef);
            }

            _allHediffsCache = [.._allHediffsCache.OrderBy(def => def.label)];

            return _allHediffsCache;
        }
    }

    public void Init()
    {
        if (CacheInited)
        {
            return;
        }

        MinCache = (float)Math.Round(Settings.Store.MultiplierCaps.min * 100);
        MaxCache = (float)Math.Round(Settings.Store.MultiplierCaps.max * 100);
        NVZeroCache = SettingsHelpers.ModToMultiPercent(LightModifiersBase.NVLightModifiers[0], true);

        NVFullCache =
            SettingsHelpers.ModToMultiPercent(LightModifiersBase.NVLightModifiers[1], false);

        PSZeroCache = SettingsHelpers.ModToMultiPercent(LightModifiersBase.PSLightModifiers[0], true);

        PSFullCache =
            SettingsHelpers.ModToMultiPercent(LightModifiersBase.PSLightModifiers[1], false);

        CacheInited = true;
    }

    /// <summary>
    ///     Sets new settings
    ///     Clears all cached stuff
    ///     Runs when opening the settings menu and when closing it
    /// </summary>
    public void DoPreWriteTasks()
    {
        // this check is required because this method is run on opening the menu
        if (CacheInited)
        {
            FieldClearer.ResetSettingsDependentFields();

            var settingsStore = Settings.Store;

            if (settingsStore.CustomCapsEnabled)
            {
                if (MinCache.HasValue)
                {
                    settingsStore.SetMinMultiplierCap(MinCache.Value);
                }

                if (MaxCache.HasValue)
                {
                    settingsStore.SetMaxMultiplierCap(MaxCache.Value);
                }
            }

            SetLightModifier(LightModifiersBase.NVLightModifiers, NVZeroCache, NVFullCache);
            SetLightModifier(LightModifiersBase.PSLightModifiers, PSZeroCache, PSFullCache);

            Classifier.ZeroLightTurningPoints = null;
            Classifier.FullLightTurningPoint = null;

            MinCache = null;
            MaxCache = null;
            NVZeroCache = null;
            NVFullCache = null;
            PSZeroCache = null;
            PSFullCache = null;
        }

        CacheInited = false;
        _allHediffsCache?.Clear();
        _headgearCache?.Clear();

        SettingsHelpers.TipStringHolder.Clear();
        Mod.Settings.ClearDrawVariables();


        if (Current.ProgramState == ProgramState.Playing)
        {
            SetDirtyAllComps();
        }
    }


    /// <summary>
    ///     So that the comps will update with the new settings, sets all the comps dirty
    /// </summary>
    public void SetDirtyAllComps()
    {
        foreach (var pawn in PawnsFinder.AllMaps_Spawned)
        {
            if (pawn?.TryGetComp<Comp_NightVision>() is { } comp)
            {
                comp.SetDirty();
            }
        }
    }

    public void Reset()
    {
        CacheInited = false;
        DoPreWriteTasks();
        Init();
    }

    private void SetLightModifier(LightModifiersBase modifier, float? zeroVal, float? fullVal)
    {
        if (zeroVal.HasValue)
        {
            modifier.Offsets[0] = SettingsHelpers.MultiPercentToMod(zeroVal.Value, true);
        }

        if (fullVal.HasValue)
        {
            modifier.Offsets[1] = SettingsHelpers.MultiPercentToMod(fullVal.Value, false);
        }
    }
}