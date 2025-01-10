// Nightvision SolarRaid_PawnGenerator.cs
// 
// 17 10 2018
// 
// 24 10 2018

using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace NightVision;

public static class SolarRaid_PawnGenerator
{
    public static List<Pawn> GeneratePawns(PawnGroupMakerParms parms, PawnGroupMaker groupMaker)
    {
        var list = new List<Pawn>();
        PawnGroupKindWorker.pawnsBeingGeneratedNow.Add(list);

        try
        {
            GeneratePawns(parms, groupMaker, list);
        }
        catch (Exception arg)
        {
            Log.Error($"Exception while generating pawn group: {arg}");

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < list.Count; i++)
            {
                list[i].Destroy();
            }

            list.Clear();
        }
        finally
        {
            PawnGroupKindWorker.pawnsBeingGeneratedNow.Remove(list);
        }

        return list;
    }


    public static void GeneratePawns(PawnGroupMakerParms parms, PawnGroupMaker groupMaker, List<Pawn> outPawns)
    {
        var canBringFood = parms.raidStrategy?.pawnsCanBringFood ?? true;

        // 15.08.2021 RW1.3 added total points parameter to .CanUsePawn
        // For now just using parms.points however unsure if this will mess other things up
        var postGear = parms.raidStrategy == null
            ? null
            : new Predicate<Pawn>(p => parms.raidStrategy.Worker.CanUsePawn(parms.points, p, outPawns));

        var madeOnePawnIncap = false;
        PawnGenUtility.cachedConvertedPawnKindDefs = new Dictionary<string, PawnKindDef>();

        foreach (var pawnGenOption in SolarRaidGroupMaker.ChoosePawnGenByConstraint(
                     parms.points,
                     groupMaker.options,
                     parms
                 ))
        {
            var kind = pawnGenOption.kind;
            var faction = parms.faction;
            var tile = parms.tile;
            var inhabitants = parms.inhabitants;

            var request = new PawnGenerationRequest(
                PawnGenUtility.ConvertDefAndStoreOld(kind),
                faction,
                PawnGenerationContext.NonPlayer,
                tile,
                false,
                false,
                false,
                true,
                true,
                1f,
                true,
                allowFood: canBringFood,
                inhabitant: inhabitants,
                certainlyBeenInCryptosleep: false,
                forceRedressWorldPawnIfFormerColonist: false,
                worldPawnFactionDoesntMatter: false,
                validatorPreGear: pa => !pa.skills.GetSkill(Defs_Rimworld.MeleeSkill).TotallyDisabled,
                validatorPostGear: postGear,
                minChanceToRedressWorldPawn: null,
                fixedBiologicalAge: null,
                fixedChronologicalAge: null,
                fixedGender: null,
                fixedLastName: null
            );

            var pawn = PawnGenerator.GeneratePawn(request);

            if (parms.forceOneDowned && !madeOnePawnIncap)
            {
                pawn.health.forceDowned = true;
                pawn.mindState.canFleeIndividual = false;
                madeOnePawnIncap = true;
            }

            PawnFinaliser(pawn);
            outPawns.Add(pawn);
        }
    }

    public static void PawnFinaliser(Pawn pawn)
    {
        var meleeSkill = pawn.skills.GetSkill(Defs_Rimworld.MeleeSkill).Level;

        if (meleeSkill < 10)
        {
            pawn.skills.GetSkill(Defs_Rimworld.MeleeSkill).Level +=
                Rand.RangeInclusive(10 - meleeSkill, 10 - meleeSkill + 5);
        }

        float[] choiceArray =
            [10 - NVGameComponent.Evilness, 5 + NVGameComponent.Evilness, 5 + NVGameComponent.Evilness];
        var indexes = new[] { 0, 1, 2 };

        var choice = indexes.RandomElementByWeight(ind => choiceArray[ind]);

        switch (choice)
        {
            case 1:

                if (PawnGenUtility.AnyPSHediffsExist.IsNotFalse())
                {
                    foreach (var bodyPartRecord in pawn.RaceProps.body.GetPartsWithTag(Defs_Rimworld.EyeTag))
                    {
                        var hediff = PawnGenUtility.GetRandomPhotosensitiveHediffDef();

                        if (hediff != null)
                        {
                            pawn.health.AddHediff(hediff, bodyPartRecord);
                        }
                    }

                    if (ApparelGenUtility.GetNullPSApparelDefByTag(pawn.kindDef.apparelTags) is { } appDef)
                    {
                        if (ApparelUtility.HasPartsToWear(pawn, appDef))
                        {
                            var apparel = ThingMaker.MakeThing(
                                appDef,
                                appDef.MadeFromStuff ? GenStuff.RandomStuffByCommonalityFor(appDef) : null
                            );

                            pawn.apparel.Wear((Apparel)apparel, false);
                        }
                    }

                    break;
                }

                goto case 2;
            case 2:

                if (ApparelGenUtility.GetGiveNVApparelDefByTag(pawn.kindDef.apparelTags) is { } nvAppDef)
                {
                    if (ApparelUtility.HasPartsToWear(pawn, nvAppDef))
                    {
                        var apparel = ThingMaker.MakeThing(
                            nvAppDef,
                            nvAppDef.MadeFromStuff ? GenStuff.RandomStuffByCommonalityFor(nvAppDef) : null
                        );
                        PawnGenerator.PostProcessGeneratedGear(apparel, pawn);
                        pawn.apparel.Wear((Apparel)apparel, false);
                    }
                }

                break;
        }

        if (!Rand.Chance(NVGameComponent.Evilness / 8f))
        {
            return;
        }

        var shield = Defs_Rimworld.ShieldDef;

        if (!ApparelUtility.HasPartsToWear(pawn, shield))
        {
            return;
        }

        var shieldBelt =
            ThingMaker.MakeThing(shield, shield.MadeFromStuff ? GenStuff.RandomStuffFor(shield) : null);
        PawnGenerator.PostProcessGeneratedGear(shieldBelt, pawn);
        pawn.apparel.Wear((Apparel)shieldBelt, false);
    }
}