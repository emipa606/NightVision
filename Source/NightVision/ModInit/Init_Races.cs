// Nightvision Init_Races.cs
// 
// 06 12 2018
// 
// 06 12 2018

using System.Collections.Generic;
using System.Linq;
using Verse;

namespace NightVision;

public partial class Initialiser

{
    private void FindAllValidRaces()
    {
        //Check for compprops so that humanlike req can be overridden in xml
        var raceDefList = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(rdef => rdef.race is { } race &&
            !rdef.IsCorpse &&
            (race.Humanlike || rdef.GetCompProperties<CompProperties_NightVision>() != null)
        );
        var RaceLightMods =
            Settings.Store.RaceLightMods ?? new Dictionary<ThingDef, Race_LightModifiers>();

        foreach (var rdef in raceDefList)
        {
            if (!RaceLightMods.TryGetValue(rdef, out var rLm) || rLm == null)
            {
                RaceLightMods[rdef] = new Race_LightModifiers(rdef);
            }

            // Note: When dictionary is loaded and calls exposedata on the saved Race_LightModifiers the def & corresponding compProps are attached
        }

        Settings.Store.RaceLightMods =
            new Dictionary<ThingDef, Race_LightModifiers>(RaceLightMods.OrderBy(pair => pair.Key.label));
    }
}