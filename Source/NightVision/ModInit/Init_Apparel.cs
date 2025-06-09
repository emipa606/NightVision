// Nightvision Init_Apparel.cs
// 
// 06 12 2018
// 
// 06 12 2018

using System.Collections.Generic;
using Verse;

namespace NightVision;

public partial class Initialiser
{
    private void FindAllValidApparel()
    {
        var headgearCategoryDef = ThingCategoryDef.Named("Headgear");
        var fullHead = Defs_Rimworld.Head;
        var eyes = Defs_Rimworld.Eyes;

        var allEyeCoveringHeadgearDefs = new HashSet<ThingDef>(
            DefDatabase<ThingDef>.AllDefsListForReading.FindAll(def => def.IsApparel
                                                                       && ((def.thingCategories?.Contains(
                                                                               headgearCategoryDef) ?? false)
                                                                           || def.apparel.bodyPartGroups.Any(bpg =>
                                                                               bpg == eyes || bpg == fullHead)
                                                                           || def.HasComp(
                                                                               typeof(Comp_NightVisionApparel)))
            )
        );
        var nvApparel = Settings.Store.NVApparel ?? new Dictionary<ThingDef, ApparelVisionSetting>();

        //Add defs that have NV comp
        foreach (var apparel in allEyeCoveringHeadgearDefs)
        {
            if (apparel.comps.Find(comp => comp is CompProperties_NightVisionApparel) is
                CompProperties_NightVisionApparel)
            {
                if (!nvApparel.TryGetValue(apparel, out var setting))
                {
                    nvApparel[apparel] = new ApparelVisionSetting(apparel);
                }
                else
                {
                    setting.InitExistingSetting(apparel);
                }
            }
            else
            {
                //This attaches a new comp to the def as a placeholder
                ApparelVisionSetting.CreateNewApparelVisionSetting(apparel);
            }
        }

        Settings.Store.NVApparel = nvApparel;
        Settings.Store.AllEyeCoveringHeadgearDefs = allEyeCoveringHeadgearDefs;
    }
}