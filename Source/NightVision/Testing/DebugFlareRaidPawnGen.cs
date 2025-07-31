// Nightvision DebugFlareRaidPawnGen.cs
// 
// 18 10 2018
// 
// 18 10 2018

using System.Linq;
using System.Text;
using LudeonTK;
using RimWorld;
using Verse;

namespace NightVision.Testing;

public class DebugFlareRaidPawnGen
{
    [DebugOutput("NightVision")]
    public static void FlareRaidPawnGroupsMade()
    {
        Dialog_DebugOptionListLister.ShowSimpleDebugMenu(
            from fac in Find.FactionManager.AllFactions
            where !fac.def.pawnGroupMakers.NullOrEmpty() && fac.def.humanlikeFaction &&
                  fac.def.techLevel >= TechLevel.Industrial
            select fac,
            fac => $"{fac.Name} ({fac.def.defName})",
            delegate(Faction fac)
            {
                var sb = new StringBuilder();
                sb.AppendLine(
                    $"Point multiplier = ;{SolarRaidGroupMaker.PointMultiplier};;Max pawn cost multiplier = ;{SolarRaidGroupMaker.MaxPawnCostMultiplier};;");
                var minPointsToGen = fac.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat);
                sb.AppendLine($"Faction =;{fac.def.defName};;Min pts to gen CombatGroup = ;{minPointsToGen};;");

                foreach (var num in /*Dialog_DebugActionsMenu*/DebugActionsUtility.PointsOptions(false))
                {
                    Action(num);
                }

                Log.Message(sb.ToString());
                return;

                void Action(float points)
                {
                    if (points < fac.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat))
                    {
                        return;
                    }

                    var originalPoints = points;
                    var tile = Find.CurrentMap.Tile;
                    points = IncidentWorker_Raid.AdjustedRaidPoints(points, PawnsArrivalModeDefOf.CenterDrop,
                        RaidStrategyDefOf.ImmediateAttack, fac, PawnGroupKindDefOf.Combat, Find.CurrentMap);
                    var pawnGroupMakerParms = new PawnGroupMakerParms
                    {
                        groupKind = PawnGroupKindDefOf.Combat,
                        tile = tile,
                        points = points,
                        faction = fac,
                        raidStrategy = RaidStrategyDefOf.ImmediateAttack
                    };
                    pawnGroupMakerParms.groupKind = PawnGroupKindDefOf.Combat;


                    var maxPawnCost = PawnGroupMakerUtility.MaxPawnCost(fac, points, RaidStrategyDefOf.ImmediateAttack,
                        PawnGroupKindDefOf.Combat);
                    sb.AppendLine(
                        $"Adjusted Points =;{pawnGroupMakerParms.points};Original points;{originalPoints};Max pawn cost;{maxPawnCost};");

                    //Points: X; MaxPawnCost:
                    //$"{};{};{};{};{};{}"
                    //150;   Mercenary_Slasher; Gladius;  Y; Apparel_FlakJacket; NV_tinted_goggles
                    var num2 = 0f;
                    SolarRaidGroupMaker.TryGetRandomPawnGroupMaker(pawnGroupMakerParms, out var groupMaker);
                    Log.Message(new string('-', 20));
                    Log.Message("Random group maker result:");
                    Log.Message($"points = {points}");

                    Log.Message(
                        $"groupMaker. = {groupMaker.options.ConvertAll(opt => opt.kind.LabelCap).ToStringSafeEnumerable()}");
                    Log.Message(new string('-', 20));


                    foreach (var pawn in SolarRaid_PawnGenerator.GeneratePawns(pawnGroupMakerParms, groupMaker)
                                 .OrderBy(pa => pa.kindDef.combatPower))
                    {
                        sb.Append($"  {pawn.kindDef.combatPower,-6:F0};{pawn.kindDef.LabelCap};");

                        if (pawn.equipment.Primary != null)
                        {
                            pawn.equipment.AllEquipmentListForReading.Aggregate(sb,
                                (builder, comps) => builder.Append(comps.def.LabelCap + ","));
                        }
                        else
                        {
                            sb.Append("no equipment");
                        }

                        sb.Append(";");

                        var wornApparel = pawn.apparel.WornApparel;
                        var torsoGear = "";
                        var eyeWear = "";
                        var shield = "";
                        foreach (var apparel in wornApparel)
                        {
                            foreach (var bodyPartGroupDef in apparel.def.apparel.bodyPartGroups)
                            {
                                if (bodyPartGroupDef == BodyPartGroupDefOf.Torso)
                                {
                                    torsoGear += $"{apparel.def.LabelCap}, ";
                                }
                                else if (bodyPartGroupDef == BodyPartGroupDefOf.Eyes)
                                {
                                    eyeWear += $"{apparel.def.LabelCap}, ";
                                }

                                if (apparel.def == Defs_Rimworld.ShieldDef && shield.NullOrEmpty())
                                {
                                    shield = "Y;";
                                }
                            }
                        }

                        torsoGear = torsoGear.NullOrEmpty() ? "shirtless" : torsoGear.TrimEnd(' ', ',');

                        eyeWear = eyeWear.NullOrEmpty() ? "not bespectacled" : eyeWear.TrimEnd(' ', ',');

                        shield = shield.NullOrEmpty() ? "N" : shield;


                        sb.Append($"{shield};{torsoGear};{eyeWear};");

                        if (pawn.health.hediffSet.hediffs.Count > 0)
                        {
                            pawn.health.hediffSet.hediffs.Aggregate(sb,
                                (builder, comps) => builder.Append(comps.def.LabelCap + ","));
                        }
                        else
                        {
                            sb.Append("no hediffs");
                        }

                        sb.AppendLine();
                        num2 += pawn.kindDef.combatPower;
                    }

                    sb.AppendLine($";;;;Final point cost;{num2};");
                    sb.AppendLine();
                }
            }
        );
    }
}