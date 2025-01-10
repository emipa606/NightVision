using JetBrains.Annotations;
using Verse;

namespace NightVision;

public class CompProperties_NightVisionApparel : CompProperties
{
    public readonly bool GrantsNightVision = false;
    public readonly bool NullifiesPhotosensitivity = false;
    public ApparelVisionSetting AppVisionSetting;

    [UsedImplicitly]
    public CompProperties_NightVisionApparel()
    {
        compClass = typeof(Comp_NightVisionApparel);
    }
}