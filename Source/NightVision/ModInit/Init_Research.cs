// Nightvision Init_Research.cs
// 
// 06 12 2018
// 
// 06 12 2018

using Verse;

namespace NightVision;

public partial class Initialiser
{
    //Adds a marker to research projects that unlock NV stuff so people know how cool I am
    private void AddNightVisionMarkerToVanillaResearch()
    {
        var complexClothing = ResearchProjectDef.Named("ComplexClothing");
        var microelectronics = ResearchProjectDef.Named("MicroelectronicsBasics");
        var powerArmour = ResearchProjectDef.Named("PoweredArmor");

        complexClothing.label += " NV+";
        microelectronics.label += " NV+";
        powerArmour.label += " NV+";

        complexClothing.description += $"\n{"NVResClothAddition".Translate()}";
        microelectronics.description += $"\n{"NVResMicroAddition".Translate()}";
        powerArmour.description += $"\n{"NVResPowerAddition".Translate()}";
    }
}