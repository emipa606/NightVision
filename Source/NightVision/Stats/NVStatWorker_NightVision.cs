using JetBrains.Annotations;

namespace NightVision;

[UsedImplicitly]
public class NVStatWorker_NightVision : NVStatWorker
{
    public NVStatWorker_NightVision()
    {
        Glow = 0f;
        StatEffectMask = ApparelFlags.GrantsNV;
        DefaultStatValue = Constants.DEFAULT_ZERO_LIGHT_MULTIPLIER;
        Acronym = Str.NV;
    }
}