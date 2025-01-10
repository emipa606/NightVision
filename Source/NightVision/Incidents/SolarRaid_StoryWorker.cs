using System.Linq;
using RimWorld;
using Verse;

namespace NightVision;

public class SolarRaid_StoryWorker
{
    public static readonly int MinTicksLeftToFireInc = 2500;
    public static int MinTicksPassedToFire = 2500;
    public static readonly IncidentDef FlareRaidDef = IncidentDef.Named("FlareRaid");
    public static readonly double MaxSunGlowForRaid = 0.2;
    public int GlobalLastFireTick = -1;
    public int LastFlareStartTick = -1;
    public int QueuedFiringTick = -1;
    public int QueuedMapID = -1;

    public void TickCheckForFlareRaid()
    {
        if (!NVGameComponent.FlareRaidIsEnabled
            || Find.Scenario.AllParts.Any(
                scenPart => scenPart is ScenPart_DisableIncident scenPartDisable &&
                            scenPartDisable.Incident == FlareRaidDef
            ))
        {
            return;
        }

        if (QueuedMapID > 0 && QueuedFiringTick <= Find.TickManager.TicksAbs)
        {
            if (!Find.World.GameConditionManager.ConditionIsActive(Defs_Rimworld.SolarFlare))
            {
                Log.Message("NVGameComp: found queued map but solar flare no longer active. Miscalculated?");
                QueuedFiringTick = -1;
                QueuedMapID = -1;
                return;
            }

            if (QueuedFiringTick < 0)
            {
                Log.Message(
                    "NVGameComp: had queued map index, and solar flare is active but queued tick was -1. Error?");
                QueuedFiringTick = -1;
                QueuedMapID = -1;
                return;
            }

            var map = Find.Maps.Find(mp => mp.uniqueID == QueuedMapID);

            QueuedFiringTick = -1;
            QueuedMapID = -1;

            if (map == null)
            {
                Log.Message("NVGameComp: MapID and Tick were both valid but map was not found. Map destroyed?");
                return;
            }

            TryFireFlareRaid(map);
        }
        else if (QueuedFiringTick > 0 && QueuedMapID < 0)
        {
            Log.Message("NVGameComp: found queued firing tick but queued mapID was -1.");
            QueuedFiringTick = -1;
            QueuedMapID = -1;
        }

        if (Find.World.GameConditionManager.GetActiveCondition(Defs_Rimworld.SolarFlare) is
                not
                {
                    Permanent: false
                } solarFlare || solarFlare.startTick == LastFlareStartTick)
        {
            return;
        }

        {
            LastFlareStartTick = solarFlare.startTick;

            if (solarFlare.TicksLeft <= MinTicksLeftToFireInc)
            {
                return;
            }

            var difficultyDef = Find.Storyteller.difficulty;

            if (!difficultyDef.allowBigThreats)
            {
                return;
            }


            var potentialTargets = Find.Maps.FindAll(map => map.IsPlayerHome);

            if (potentialTargets.Count == 0)
            {
                return;
            }

            var hourCount = 0;
            // use tolist to force eval
            var anony = potentialTargets
                .Select(target => new
                {
                    mapID = target.uniqueID,
                    hours = CalcPotentialHoursToFire(target, solarFlare.TicksLeft, ref hourCount)
                }).Where(anon => anon.hours != null).ToList();


            if (
                hourCount == 0)
            {
                return;
            }

            var ticksTillFireInHourTerms = -1250 + Rand.Range(0, hourCount * 2500);

            var firingTick = -1;
            var mapId = -1;

            foreach (var mapHours in anony)
            {
                foreach (var hour in mapHours.hours)
                {
                    if (ticksTillFireInHourTerms < 1250)
                    {
                        firingTick = (hour * 2500) + ticksTillFireInHourTerms;

                        break;
                    }

                    ticksTillFireInHourTerms -= 2500;
                }

                if (firingTick <= 0)
                {
                    continue;
                }

                mapId = mapHours.mapID;

                break;
            }

            if (firingTick < 0 || mapId < 0)
            {
                Log.Message(new string('-', 20));
                Log.Message("NVGameComponent");
                Log.Message("TickCheckForFlareRaid");
                Log.Message("No tick or map index found when there should be one.");
                Log.Message(new string('-', 20));

                return;
            }


            QueuedFiringTick = firingTick + Find.TickManager.TicksAbs;
            QueuedMapID = mapId;
        }
    }

    public void TryFireFlareRaid(Map map)
    {
        //int ticksGame = Find.TickManager.TicksGame;

        //if (GlobalLastFireTick > 0 && (ticksGame - GlobalLastFireTick) / 60000f < FlareRaidDef.minRefireDays)
        //{
        //    return false;
        //}
        if (!Rand.Chance(SolarRaid_IncidentWorker.ChanceForFlareRaid(map)))
        {
            return;
        }

        var newParms = StorytellerUtility.DefaultParmsNow(FlareRaidDef.category, map);


        if (!FlareRaidDef.Worker.CanFireNow(newParms))
        {
            return;
        }

        Find.Storyteller.TryFire(new FiringIncident(FlareRaidDef, null, newParms));
    }

    public int[] CalcPotentialHoursToFire(Map map, int flareTicks, ref int hourCount)
    {
        var currentTick = Find.TickManager.TicksAbs;

        var numHoursFromNow = flareTicks / 2500;

        if (numHoursFromNow == 0)
        {
            return null;
        }

        hourCount += numHoursFromNow;
        var result = new int[numHoursFromNow];

        for (var hoursFromNow = 1; hoursFromNow <= numHoursFromNow; hoursFromNow++)
        {
            if (GenCelestial.CelestialSunGlow(map, currentTick + (2500 * hoursFromNow)) < MaxSunGlowForRaid)
            {
                result[hoursFromNow - 1] = hoursFromNow;
            }
        }

        return result;
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref LastFlareStartTick, "lastFlareStartTick", -1);
        Scribe_Values.Look(ref QueuedFiringTick, "queuedFiringTick", -1);
        Scribe_Values.Look(ref QueuedMapID, "queuedMapID", -1);
    }
}