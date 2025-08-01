﻿// Nightvision DebugFlareRaidPawnGenXml.cs
// 
// 18 10 2018
// 
// 18 10 2018

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using LudeonTK;
using RimWorld;
using Verse;

namespace NightVision.Testing;

public class DebugFlareRaidPawnGenXml
{
    public static pawnGenTrial CurrentTrial;

    private static RaidStrategyDef GetSmart()
    {
        foreach (var raidStrategyDef in DefDatabase<RaidStrategyDef>.AllDefs)
        {
            if (raidStrategyDef.defName == "ImmediateAttackSmart")
            {
                return raidStrategyDef;
            }
        }

        return RaidStrategyDefOf.ImmediateAttack;
    }

    [DebugOutput("Nightvision")]
    public static void FlareRaidPawnGroupsMadeToXml()
    {
        Dialog_DebugOptionListLister.ShowSimpleDebugMenu(
            new List<int>
            {
                11,
                12,
                13,
                14,
                21,
                22,
                23,
                24,
                31,
                32,
                33,
                34,
                41,
                42,
                43,
                44
            },
            i => $"Points x{i / 10} | MaxPawn x{i % 10}",
            delegate(int multi)
            {
                var trialData = new pawnGenTrial();
                SolarRaidGroupMaker.PointMultiplier = multi / 10;
                SolarRaidGroupMaker.MaxPawnCostMultiplier = multi % 10;

                trialData.pointMultiplier = SolarRaidGroupMaker.PointMultiplier;
                trialData.maxPawnCostMultiplier = SolarRaidGroupMaker.MaxPawnCostMultiplier;
                trialData.trialID = $"{multi}PawnGenTrial{Rand.Int}";

                var factions = Find.FactionManager.AllFactions.Where(fac =>
                    !fac.def.pawnGroupMakers.NullOrEmpty() && fac.def.humanlikeFaction &&
                    fac.def.techLevel >= TechLevel.Industrial
                ).ToList();

                var numTrials = factions.Count;
                trialData.trial = new pawnGenTrialTrial[numTrials];

                for (var ind = 0; ind < factions.Count; ind++)
                {
                    var fac = factions[ind];
                    var trial = new pawnGenTrialTrial
                    {
                        factionName = fac.def.LabelCap,
                        minPointsToGenCombatGroup =
                            fac.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat)
                    };


                    var floats = DebugActionsUtility.PointsOptions(false).ToList();
                    trial.groupGenerated = new pawnGenTrialTrialGroupGenerated[floats.Count];

                    for (var index = 0; index < floats.Count; index++)
                    {
                        var num = floats[index];
                        trial.groupGenerated[index] = groupGenerated(fac, num);
                    }

                    trialData.trial[ind] = trial;
                }

                var mySerializer = new
                    XmlSerializer(typeof(pawnGenTrial));

                using var writer = new StreamWriter($"pawnGenData{Rand.Int}.xml");
                mySerializer.Serialize(writer, trialData);
            }
        );
    }

    private static pawnGenTrialTrialGroupGenerated groupGenerated(Faction fac, float points)
    {
        if (points < fac.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat))
        {
            return null;
        }

        var groupGen = new pawnGenTrialTrialGroupGenerated { originalPoints = points };

        points = IncidentWorker_Raid.AdjustedRaidPoints(
            points,
            PawnsArrivalModeDefOf.CenterDrop,
            GetSmart(),
            fac,
            PawnGroupKindDefOf.Combat,
            Find.CurrentMap
        );

        groupGen.modifiedPoints = points;

        var pawnGroupMakerParms = new PawnGroupMakerParms
        {
            groupKind = PawnGroupKindDefOf.Combat,
            tile = Find.CurrentMap.Tile,
            points = points,
            faction = fac,
            raidStrategy = GetSmart()
        };
        pawnGroupMakerParms.groupKind = PawnGroupKindDefOf.Combat;
        Log.Message($"raid strat. = {pawnGroupMakerParms.raidStrategy}");


        var maxPawnCost = SolarRaidGroupMaker.MaxPawnCostMultiplier * PawnGroupMakerUtility.MaxPawnCost(
            fac,
            points,
            GetSmart(),
            PawnGroupKindDefOf.Combat
        );

        groupGen.maxPawnCost = maxPawnCost;


        SolarRaidGroupMaker.TryGetRandomPawnGroupMaker(pawnGroupMakerParms, out var groupMaker);
        var pawns = SolarRaid_PawnGenerator.GeneratePawns(pawnGroupMakerParms, groupMaker)
            .OrderBy(pa => pa.kindDef.combatPower).ToList();

        var pawnCount = pawns.Count;
        groupGen.pawn = new pawnGenTrialTrialGroupGeneratedPawn[pawnCount];
        var pointsSpent = 0f;

        for (var index = 0; index < pawnCount; index++)
        {
            var pawn = pawns[index];
            var pawnData = new pawnGenTrialTrialGroupGeneratedPawn
            {
                label = pawn.KindLabel,
                combatPower = (int)pawn.kindDef.combatPower,
                primaryEq = pawn.equipment.Primary != null
                    ? pawn.equipment.Primary.LabelCapNoCount
                    : "no weapon"
            };


            var wornApparel = pawn.apparel.WornApparel;
            var torsoGear = "";
            var eyeWear = "";

            foreach (var apparel in wornApparel)
            {
                foreach (var bpgd in apparel.def.apparel.bodyPartGroups)
                {
                    if (bpgd == BodyPartGroupDefOf.Torso)
                    {
                        torsoGear += $"{apparel.def.LabelCap}, ";
                    }
                    else if (bpgd == BodyPartGroupDefOf.Eyes)
                    {
                        eyeWear += $"{apparel.def.LabelCap}, ";
                    }

                    if (apparel.def == Defs_Rimworld.ShieldDef)
                    {
                        pawnData.hasShield = true;
                    }
                }
            }

            pawnData.apparelChest = torsoGear.NullOrEmpty() ? "shirtless" : torsoGear.TrimEnd(' ', ',');

            pawnData.apparelHead = eyeWear.NullOrEmpty() ? "not bespectacled" : eyeWear.TrimEnd(' ', ',');


            pawnData.hediffs = pawn.health.hediffSet.hediffs.Where(hd => hd is not Hediff_MissingPart)
                .Select(hdf => hdf.LabelCap).ToCommaList();
            pointsSpent += pawn.kindDef.combatPower;

            groupGen.pawn[index] = pawnData;
        }

        groupGen.pointsSpent = pointsSpent;
        return groupGen;
    }
}