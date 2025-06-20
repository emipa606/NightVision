﻿using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Verse;

namespace NightVision;

[NVHasSettingsDependentField]
public static class ApparelGenUtility
{
    private static Lookup<string, ThingDef> NullPSApparelDefsByTag;

    [NVSettingsDependentField] private static TriBool AnyNullPSApparelExists = TriBool.Undefined;


    private static Lookup<string, ThingDef> GiveNVApparelDefsByTag;

    [NVSettingsDependentField] private static TriBool AnyGiveNVApparelExists = TriBool.Undefined;

    private static void BuildNullPSApparelLookup()
    {
        var nvApparel = Settings.Store.NVApparel;

        if (nvApparel.Where(appset => appset.Value.NullifiesPS && appset.Key.IsApparel)
                .SelectMany(appset => appset.Key.apparel.tags.Select(tag => new { tag, thingDef = appset.Key }))
                .ToLookup(anonT => anonT.tag, anonT => anonT.thingDef) is not Lookup<string, ThingDef> list ||
            list.Count == 0)
        {
            AnyNullPSApparelExists.MakeFalse();
        }
        else
        {
            AnyNullPSApparelExists.MakeTrue();
            NullPSApparelDefsByTag = list;
        }
    }

    [CanBeNull]
    public static ThingDef GetNullPSApparelDefByTag(List<string> tagsList)
    {
        if (AnyNullPSApparelExists.IsUndefined())
        {
            BuildNullPSApparelLookup();
        }

        if (AnyNullPSApparelExists.IsFalse())
        {
            return null;
        }

        //because better stuff is normally at the end
        tagsList.Reverse();
        foreach (var tag in tagsList)
        {
            if (NullPSApparelDefsByTag[tag].Any())
            {
                return NullPSApparelDefsByTag[tag].RandomElement();
            }
        }

        return null;
    }

    private static void BuildGiveNVApparelLookup()
    {
        var nvApparel = Settings.Store.NVApparel;

        if (nvApparel.Where(appset => appset.Value.GrantsNV && appset.Key.IsApparel)
                .SelectMany(appset => appset.Key.apparel.tags.Select(tag => new { tag, thingDef = appset.Key }))
                .ToLookup(anonT => anonT.tag, anonT => anonT.thingDef) is not Lookup<string, ThingDef> list ||
            list.Count == 0)
        {
            AnyGiveNVApparelExists.MakeFalse();
        }
        else
        {
            AnyGiveNVApparelExists.MakeTrue();
            GiveNVApparelDefsByTag = list;
        }
    }

    [CanBeNull]
    public static ThingDef GetGiveNVApparelDefByTag(List<string> tagsList)
    {
        if (AnyGiveNVApparelExists.IsUndefined())
        {
            BuildGiveNVApparelLookup();
        }

        if (AnyGiveNVApparelExists.IsFalse())
        {
            return null;
        }

        //because better stuff is normally at the end
        tagsList.Reverse();
        foreach (var tag in tagsList)
        {
            if (GiveNVApparelDefsByTag[tag].Any())
            {
                return GiveNVApparelDefsByTag[tag].RandomElement();
            }
        }

        return null;
    }
}