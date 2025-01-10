using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace NightVision;

public static class SolarRaidGroupMaker
{
    public static int PointMultiplier = 1;

    public static int MaxPawnCostMultiplier = 4;

    private static readonly SimpleCurve WeightingPreferPawnsCloseToHighestCost =
    [
        new CurvePoint(0.4f, 0.01f),
        new CurvePoint(0.5f, 0.3f),
        new CurvePoint(0.7f, 1f)
    ];

    public static void TryGetRandomPawnGroupMaker(PawnGroupMakerParms parms, out PawnGroupMaker pawnGroupMaker)
    {
        if (parms.faction.def.pawnGroupMakers.NullOrEmpty())
        {
            Log.Error(
                $"Faction {parms.faction} of def {parms.faction.def} has no any PawnGroupMakers."
            );
        }

        if (parms.seed != null)
        {
            Rand.PushState(parms.seed.Value);
        }

        var source =
            from gm in parms.faction.def.pawnGroupMakers
            where gm.kindDef == parms.groupKind && gm.CanGenerateFrom(parms)
            select gm;

        source.TryRandomElementByWeight(gm => gm.commonality, out pawnGroupMaker);

        if (!source.Any())
        {
            Log.Message("Found no pawn groups fit for purpose");
        }

        if (parms.seed != null)
        {
            Rand.PopState();
        }
    }

    private static bool MeleeConstraint(PawnGenOption pawnGenOption)
    {
        return pawnGenOption.kind.weaponTags.Count > 0
               && pawnGenOption.kind.weaponTags.Any(str => str.Contains("Melee") || str.Contains("melee"));
    }

    public static IEnumerable<PawnGenOption> ChoosePawnGenByConstraint
        (float pointsTotal, List<PawnGenOption> options, PawnGroupMakerParms groupParms)
    {
        if (groupParms.seed != null)
        {
            Rand.PushState(groupParms.seed.Value);
        }

        var maxPawnCost = MaxPawnCostMultiplier
                          * PawnGroupMakerUtility.MaxPawnCost(
                              groupParms.faction,
                              pointsTotal,
                              groupParms.raidStrategy,
                              groupParms.groupKind
                          );

        var candidates = new List<PawnGenOption>();
        var bestOptions = new List<PawnGenOption>();
        var remTotal = pointsTotal * PointMultiplier;
        var foundLeader = false;
        var highestCost = -1f;

        for (;;)
        {
            candidates.Clear();

            foreach (var pawnGenOption in options)
            {
                if (!(pawnGenOption.Cost <= remTotal))
                {
                    continue;
                }

                if (pawnGenOption.Cost <= maxPawnCost)
                {
                    highestCost = HighestCost(
                        groupParms,
                        pawnGenOption,
                        bestOptions,
                        foundLeader,
                        highestCost,
                        candidates
                    );
                }
            }

            if (candidates.Count == 0)
            {
                break;
            }

            var pawnGenOption2 = candidates.RandomElementByWeight(WeightSelector);
            bestOptions.Add(pawnGenOption2);
            remTotal -= pawnGenOption2.Cost;

            if (pawnGenOption2.kind.factionLeader)
            {
                foundLeader = true;
            }

            continue;

            float WeightSelector(PawnGenOption gr)
            {
                var selectionWeight = gr.selectionWeight;

                return selectionWeight * WeightingPreferPawnsCloseToHighestCost.Evaluate(gr.Cost / highestCost);
            }
        }

        if (bestOptions.Count == 1 && remTotal > pointsTotal / 2f)
        {
            Log.Warning(
                $"Used only {pointsTotal - remTotal} / {pointsTotal} points generating for {groupParms.faction}"
            );
        }

        if (groupParms.seed != null)
        {
            Rand.PopState();
        }

        return bestOptions;
    }

    private static float HighestCost
    (
        PawnGroupMakerParms groupParms, PawnGenOption pawnGenOption, List<PawnGenOption> bestOptions, bool flag,
        float highestCost,
        List<PawnGenOption> candidates
    )
    {
        if (!pawnGenOption.kind.isFighter)
        {
            return highestCost;
        }

        var bestOptionsWithXeno = new List<PawnGenOptionWithXenotype>();
        foreach (var bestOption in bestOptions)
        {
            bestOptionsWithXeno.Add(new PawnGenOptionWithXenotype(bestOption, XenotypeDefOf.Baseliner, 1));
        }

        // 16.08.2021 1.3 RW added required parameter to CanUsePawnGenOption for points
        // Using groupParms.points though unsure of effects
        if (groupParms.raidStrategy != null
            && !groupParms.raidStrategy.Worker.CanUsePawnGenOption(groupParms.points, pawnGenOption,
                bestOptionsWithXeno))
        {
            return highestCost;
        }

        if (groupParms.dontUseSingleUseRocketLaunchers && pawnGenOption.kind.weaponTags.Contains("GunHeavy"))
        {
            return highestCost;
        }

        if (!MeleeConstraint(pawnGenOption))
        {
            return highestCost;
        }

        if (flag && pawnGenOption.kind.factionLeader)
        {
            return highestCost;
        }

        if (pawnGenOption.Cost > highestCost)
        {
            highestCost = pawnGenOption.Cost;
        }

        candidates.Add(pawnGenOption);

        return highestCost;
    }
}