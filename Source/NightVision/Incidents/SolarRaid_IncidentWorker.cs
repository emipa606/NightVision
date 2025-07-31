// Nightvision SolarRaid_IncidentWorker.cs
// 
// 17 10 2018
// 
// 18 10 2018

using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace NightVision;

[UsedImplicitly]
public class SolarRaid_IncidentWorker : IncidentWorker_RaidEnemy
{
    private static readonly PawnsArrivalModeDef ForcedArriveMode = PawnsArrivalModeDefOf.CenterDrop;

    public override bool FactionCanBeGroupSource(Faction f, IncidentParms parms, bool desperate = false)
    {
        var map = (Map)parms.target;

        return !f.IsPlayer
               && !f.defeated
               && (desperate
                   || f.def.allowedArrivalTemperatureRange.Includes(map.mapTemperature.OutdoorTemp)
                   && f.def.allowedArrivalTemperatureRange.Includes(map.mapTemperature.SeasonalTemp))
               && f.HostileTo(Faction.OfPlayer)
               && (desperate || GenDate.DaysPassed >= f.def.earliestRaidDays)
               && f.def.techLevel >= ForcedArriveMode.minTechLevel;
    }

    protected override bool CanFireNowSub(IncidentParms parms)
    {
        return Find.World.GameConditionManager.ConditionIsActive(Defs_Rimworld.SolarFlare)
               && (parms.faction != null || CandidateFactions(parms).Any());
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        parms.raidArrivalMode = ForcedArriveMode;

        ResolveRaidPoints(parms);

        if (!TryResolveRaidFaction(parms))
        {
            Log.Message("Failed solar raid: no faction found.");

            return false;
        }

        var combat = Defs_Rimworld.CombatGroup;
        ResolveRaidStrategy(parms, combat);

        if (!parms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(parms))
        {
            return false;
        }

        parms.points = AdjustedRaidPoints(
            parms.points,
            parms.raidArrivalMode,
            parms.raidStrategy,
            parms.faction,
            combat,
            parms.target as Map
        );

        var defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(
            combat,
            parms
        );

        SolarRaidGroupMaker.TryGetRandomPawnGroupMaker(defaultPawnGroupMakerParms, out var pawnGroupMaker);

        var list = SolarRaid_PawnGenerator.GeneratePawns(defaultPawnGroupMakerParms, pawnGroupMaker).ToList();

        if (list.Count == 0)
        {
            return false;
        }

        parms.raidArrivalMode.Worker.Arrive(list, parms);
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"Points = {parms.points:F0}");

        foreach (var pawn in list)
        {
            var str = pawn.equipment?.Primary == null ? "unarmed" : pawn.equipment.Primary.LabelCap;
            stringBuilder.AppendLine($"{pawn.KindLabel} - {str}");
        }

#if RW10
            string letterLabel = GetLetterLabel(parms: parms);
            string letterText = GetLetterText(parms: parms, pawns: list);
#else
        TaggedString letterLabel = GetLetterLabel(parms);
        TaggedString letterText = GetLetterText(parms, list);
#endif

        PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(
            list,
            ref letterLabel,
            ref letterText,
            GetRelatedPawnsInfoLetterText(parms),
            true
        );

        var list2 = new List<TargetInfo>();

        if (parms.pawnGroups != null)
        {
            var list3 = IncidentParmsUtility.SplitIntoGroups(list, parms.pawnGroups);
            var list4 = list3.MaxBy(x => x.Count);

            if (list4.Any())
            {
                list2.Add(list4[0]);
            }

            foreach (var pawns in list3)
            {
                if (pawns == list4)
                {
                    continue;
                }

                if (pawns.Any())
                {
                    list2.Add(pawns[0]);
                }
            }
        }
        else if (list.Any())
        {
            list2.Add(list[0]);
        }

        Find.LetterStack.ReceiveLetter(
            letterLabel,
            letterText,
            GetLetterDef(),
            list2,
            parms.faction,
            debugInfo: stringBuilder.ToString()
        );

        parms.raidStrategy.Worker.MakeLords(parms, list);
        LessonAutoActivator.TeachOpportunity(ConceptDefOf.EquippingWeapons, OpportunityType.Critical);

        if (!PlayerKnowledgeDatabase.IsComplete(ConceptDefOf.ShieldBelts))
        {
            foreach (var pawn2 in list)
            {
                if (!Enumerable.Any(pawn2.apparel.WornApparel, ap => ap.TryGetComp<CompShield>() != null))
                {
                    continue;
                }

                LessonAutoActivator.TeachOpportunity(ConceptDefOf.ShieldBelts, OpportunityType.Critical);

                break;
            }
        }

        Find.TickManager.slower.SignalForceNormalSpeedShort();
        Find.StoryWatcher.statsRecord.numRaidsEnemy++;

        return true;
    }

    protected override bool TryResolveRaidFaction(IncidentParms parms)
    {
        var map = (Map)parms.target;

        if (parms.faction != null)
        {
            return true;
        }

        var num = parms.points;

        if (num <= 0f)
        {
            num = 999999f;
        }

        return PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroup(
                   num,
                   out parms.faction,
                   f => FactionCanBeGroupSource(f, parms),
                   false,
                   true,
                   true,
                   false
               )
               || PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroup(
                   num,
                   out parms.faction,
                   f => FactionCanBeGroupSource(f, parms, true),
                   false,
                   true,
                   true,
                   false
               );
    }

#if RW10
        protected override void ResolveRaidStrategy(IncidentParms parms, PawnGroupKindDef groupKind)
        {
            if (parms.raidStrategy != null)
            {
                return;
            }

            var map = (Map)parms.target;

            if (!(from d in DefDatabase<RaidStrategyDef>.AllDefs where d.Worker.CanUseWith(parms: parms, groupKind: groupKind) select d)
                .TryRandomElementByWeight(
                    weightSelector: d => d.Worker.SelectionWeight(map: map, basePoints: parms.points),
                    result: out parms.raidStrategy
                ))
            {
                parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
            }
        }
#else
    public override void ResolveRaidStrategy(IncidentParms parms, PawnGroupKindDef groupKind)
    {
        if (parms.raidStrategy != null)
        {
            return;
        }

        var map = (Map)parms.target;

        if (!(from d in DefDatabase<RaidStrategyDef>.AllDefs where d.Worker.CanUseWith(parms, groupKind) select d)
            .TryRandomElementByWeight(
                d => d.Worker.SelectionWeight(map, parms.points),
                out parms.raidStrategy
            ))
        {
            parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
        }
    }
#endif

    protected override string GetLetterLabel(IncidentParms parms)
    {
        return parms.raidStrategy.letterLabelEnemy;
    }

    protected override string GetLetterText(IncidentParms parms, List<Pawn> pawns)
    {
        string text = "NightVisionFlareArrivalText".Translate(parms.faction.def.pawnsPlural, parms.faction.Name);
        text += "\n\n";
        text += "NightVisionFlarePawnText".Translate();
        var pawn = pawns.Find(x => x.Faction.leader == x);

        if (pawn == null)
        {
            return text;
        }

        text += "\n\n";

        text += "EnemyRaidLeaderPresent".Translate(
            pawn.Faction.def.pawnsPlural,
            pawn.LabelShort,
            pawn.Named("LEADER")
        );

        return text;
    }

    protected override LetterDef GetLetterDef()
    {
        return LetterDefOf.ThreatBig;
    }

    protected override string GetRelatedPawnsInfoLetterText(IncidentParms parms)
    {
        return "LetterRelatedPawnsRaidEnemy".Translate(Faction.OfPlayer.def.pawnsPlural, parms.faction.def.pawnsPlural);
    }

    public static float ChanceForFlareRaid(Map target)
    {
        return SolarRaid_StoryWorker.FlareRaidDef.baseChance;
    }
}