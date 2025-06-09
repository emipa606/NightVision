// Nightvision Settings.cs
// 
// 03 08 2018
// 
// 16 10 2018

using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace NightVision;

public class Settings : ModSettings
{
    public static Settings Instance;
    private readonly SettingsCache _cache;
    private readonly Storage_Combat _combatStore;

    private readonly Storage _store;
    private readonly List<TabRecord> _tabsList = [];
    public readonly bool CEDetected = false;
    private ApparelTab _apparelTab;
    private CombatTab _combatTab;
    private DebugTab _debugTab;
    private GeneralTab _generalTab;
    private HediffTab _hediffTab;
    private bool _isWindowSetup;
    private RaceTab _raceTab;

    // tabs
    private Tab _tab;

    // draw rects
    private Rect lastRect;
    private Rect menuRect;
    private Rect tabRect;


    [UsedImplicitly]
    public Settings()
    {
        Instance = this;
        _store = new Storage();
        _combatStore = new Storage_Combat();
        _cache = new SettingsCache();
    }

    public static Storage Store => Instance._store;
    public static Storage_Combat CombatStore => Instance._combatStore;
    public static SettingsCache Cache => Instance._cache;


    public override void ExposeData()
    {
        base.ExposeData();
        Cache.DoPreWriteTasks();
        Store.ExposeSettings();
        CombatStore.LoadSaveCommit();
    }

    public void Initialise()
    {
        var initialise = new Initialiser();
        initialise.Startup();

        if (Store.NullRefWhenLoading)
        {
            Write();
        }
    }

    private List<TabRecord> GenerateTabs()
    {
        _generalTab ??= new GeneralTab();

        _combatTab ??= new CombatTab();

        _raceTab ??= new RaceTab();

        _apparelTab ??= new ApparelTab();

        _hediffTab ??= new HediffTab();

        var tabsList = new List<TabRecord>
        {
            new(
                "NVGeneralTab".Translate(),
                delegate { _tab = Tab.General; },
                () => _tab == Tab.General
            ),
            new(
                "NVCombat".Translate(),
                delegate { _tab = Tab.Combat; },
                () => _tab == Tab.Combat
            ),
            new(
                "NVRaces".Translate(),
                delegate { _tab = Tab.Races; },
                () => _tab == Tab.Races
            ),
            new(
                "NVApparel".Translate(),
                delegate { _tab = Tab.Apparel; },
                () => _tab == Tab.Apparel
            ),
            new(
                "NVHediffs".Translate(),
                delegate { _tab = Tab.Bionics; },
                () => _tab == Tab.Bionics
            )
        };


        if (!Prefs.DevMode)
        {
            return tabsList;
        }

        _debugTab ??= new DebugTab();

        tabsList.Add(
            new TabRecord(
                "NVDebugTab".Translate(),
                delegate { _tab = Tab.Debug; },
                () => _tab == Tab.Debug
            )
        );

        return tabsList;
    }

    private void InitialiseWindow(Rect inRect)
    {
        Cache.Init();

        inRect.yMin += 32f;
        menuRect = inRect;
        tabRect = inRect.ContractedBy(17f);
    }

    public void DoSettingsWindowContents(Rect inRect)
    {
        if (!_isWindowSetup || lastRect != inRect)
        {
            lastRect = inRect;
            InitialiseWindow(inRect);
            _isWindowSetup = true;
            Log.Message($"Menurect: {menuRect.width}x{menuRect.height}");
        }

        Widgets.DrawMenuSection(menuRect);
        TabDrawer.DrawTabs(menuRect, GenerateTabs());

        GUI.BeginGroup(tabRect);
        var font = Text.Font;
        var anchor = Text.Anchor;
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.MiddleCenter;

        switch (_tab)
        {
            default:
                _generalTab.DrawTab(tabRect);

                break;
            case Tab.Combat:
                _combatTab.DrawTab(tabRect);
                break;
            case Tab.Races:
                _raceTab.DrawTab(tabRect);

                break;
            case Tab.Apparel:
                _apparelTab.DrawTab(tabRect);

                break;
            case Tab.Bionics:
                _hediffTab.DrawTab(tabRect);

                break;
            case Tab.Debug:
                _debugTab.DrawTab(tabRect);

                break;
        }

        Text.Font = font;
        Text.Anchor = anchor;
        GUI.EndGroup();
    }

    public void ClearDrawVariables()
    {
        /*_debugTab.Clear();
        _apparelTab.Clear();
        _raceTab.Clear();
        _generalTab.Clear();
        _combatTab.Clear();
        _hediffTab.Clear();
        initialised = false;*/
        _debugTab = null;
        _apparelTab = null;
        _raceTab = null;
        _generalTab = null;
        _combatTab = null;
        _hediffTab = null;
        _isWindowSetup = false;
    }
}