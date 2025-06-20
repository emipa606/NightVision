﻿// Nightvision NVGameComponent.cs
// 
// 19 10 2018
// 
// 19 10 2018

using JetBrains.Annotations;
using Verse;

namespace NightVision;

[UsedImplicitly]
public class NVGameComponent : GameComponent
{
    [UsedImplicitly] public static NVGameComponent Instance;

    public static bool FlareRaidIsEnabled = true;

    private SolarRaid_StoryWorker SolarRaidStoryWorker;

    public NVGameComponent(Game game)
    {
        Instance = this;
    }

    // 15.08.21 - RW1.3 made Difficulty.difficulty obsolete
    // Evilness originally = difficulty * 2
    // Rather than spend too long rethinking the entire value, simulate previous results using Difficulty.threatscale
    // 
    public static float Evilness =>
        //TODO fix
        5;

    //return GenMath.LerpDouble(0.1f, 2.2f, 0, 10, Find.Storyteller.difficulty?.threatScale ?? 1);
    public override void GameComponentTick()
    {
        if (FlareRaidIsEnabled && SolarRaidStoryWorker == null)
        {
            SolarRaidStoryWorker = new SolarRaid_StoryWorker();
        }
        else if (!FlareRaidIsEnabled && SolarRaidStoryWorker != null)
        {
            SolarRaidStoryWorker = null;
        }

        SolarRaidStoryWorker?.TickCheckForFlareRaid();
    }


    public override void ExposeData()
    {
        base.ExposeData();
        if (FlareRaidIsEnabled && Scribe.mode == LoadSaveMode.Saving)
        {
            SolarRaidStoryWorker?.ExposeData();
        }
        else if (FlareRaidIsEnabled && Scribe.mode == LoadSaveMode.LoadingVars)
        {
            SolarRaidStoryWorker = new SolarRaid_StoryWorker();
            SolarRaidStoryWorker.ExposeData();
        }
    }

    public override void FinalizeInit()
    {
        if (FlareRaidIsEnabled)
        {
            SolarRaidStoryWorker = new SolarRaid_StoryWorker();
        }

        base.FinalizeInit();
    }
}