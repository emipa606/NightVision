using Verse;

namespace NightVision;

public static class CurrentStrike
{
    public static float GlowFactor = Constants.TRIVIAL_FACTOR;
    public static float GlowDiff;

    public static bool SurpAtkSuccess => !CombatHelpers.ChanceOfSurpriseAttFactor.ApproxEq(0) &&
                                         Rand.Chance(CombatHelpers.SurpriseAttackChance(GlowDiff));
}