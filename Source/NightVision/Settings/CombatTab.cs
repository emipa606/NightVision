﻿// Nightvision CombatTab.cs
// 
// 24 10 2018
// 
// 24 10 2018

using UnityEngine;
using Verse;

namespace NightVision;

public class CombatTab
{
    private readonly float IntSliderHeight = 28f;

    private readonly float LineHeight = Text.LineHeight;

    private readonly float texRextXMod = 6f;

    private readonly float texRextYMod = 24f;
    private readonly float VerticalSpacing = 2f;
    private string[] bestAndWorstRangedCd = new string[2];

    private float MeleeSubListingHeight = 100f;

    private float RangedSubListingHeight = 100f;

    public string surpBuffer;


    private string[] BestAndWorstRangedCd
    {
        get
        {
            if (!bestAndWorstRangedCd[0].NullOrEmpty() || !bestAndWorstRangedCd[1].NullOrEmpty())
            {
                return bestAndWorstRangedCd;
            }

            var caps = Settings.Store.MultiplierCaps;
            bestAndWorstRangedCd[0] = (1 / caps.max).ToStringPercent();
            bestAndWorstRangedCd[1] = (1 / caps.min).ToStringPercent();

            return bestAndWorstRangedCd;
        }
    }

    public void DrawTab(Rect inRect)
    {
        var combatStore = Settings.CombatStore;
        var tabRect = inRect.AtZero();
        var anchor = Text.Anchor;
        Text.Anchor = TextAnchor.MiddleLeft;
        var listing = new Listing_Standard(GameFont.Small) { verticalSpacing = VerticalSpacing };
        listing.Begin(tabRect);
        var subListing = listing.SubListing(LineHeight + 12f);
        subListing.Gap(6f);
        subListing.CheckboxLabeled(combatStore.CombatFeaturesEnabled.Label,
            ref combatStore.CombatFeaturesEnabled.TempValue);
        subListing.Gap(6f);
        listing.EndSection(subListing);

        if (combatStore.CombatFeaturesEnabled.TempValue)
        {
            listing.Gap();
            subListing = listing.SubListing((LineHeight * 2) + IntSliderHeight + 12f);
            subListing.Gap(6f);
            subListing.Label(combatStore.HitCurviness.Label);
            subListing.Gap(6f);
            var rowRect = subListing.GetRect(IntSliderHeight);
            var hitCurStr = combatStore.HitCurviness.TempValue.ToString();
            float hitCurNum = combatStore.HitCurviness.TempValue;
            combatStore.HitCurviness.TempValue = (int)Widgets.HorizontalSlider(rowRect, hitCurNum, 1f, 5f, true,
                hitCurStr, "Flat", "Extreme Changes", 1f);
            TooltipHandler.TipRegion(rowRect, combatStore.HitCurviness.Tooltip);
            DrawIndicator(rowRect, 1f / 4);
            subListing.Gap(6f);
            listing.EndSection(subListing);


            listing.Gap();
            subListing = listing.SubListing(RangedSubListingHeight);
            RangedSubListingHeight = 0;
            subListing.Gap();
            RangedSubListingHeight += 12f;
            subListing.CheckboxLabeled(combatStore.RangedHitEffectsEnabled.Label,
                ref combatStore.RangedHitEffectsEnabled.TempValue, combatStore.RangedHitEffectsEnabled.Tooltip);
            RangedSubListingHeight += Text.LineHeight + VerticalSpacing;
            subListing.ShortGapLine();
            RangedSubListingHeight += 12f;
            subListing.CheckboxLabeled(combatStore.RangedCooldownEffectsEnabled.Label,
                ref combatStore.RangedCooldownEffectsEnabled.TempValue,
                combatStore.RangedCooldownEffectsEnabled.Tooltip);
            RangedSubListingHeight += Text.LineHeight + VerticalSpacing;
            if (combatStore.RangedCooldownEffectsEnabled.TempValue)
            {
                subListing.ShortGapLine();
                RangedSubListingHeight += 12f;
                subListing.CheckboxLabeled(
                    $"{combatStore.RangedCooldownLinkedToCaps.Label}[best: {BestAndWorstRangedCd[0]}, worst: {BestAndWorstRangedCd[1]}]",
                    ref combatStore.RangedCooldownLinkedToCaps.TempValue,
                    combatStore.RangedCooldownLinkedToCaps.Tooltip);
                RangedSubListingHeight += Text.LineHeight + VerticalSpacing;
                if (!combatStore.RangedCooldownLinkedToCaps.TempValue)
                {
                    subListing.ShortGapLine();
                    RangedSubListingHeight += 12f;
                    subListing.Label(combatStore.RangedCooldownMinAndMax.Label,
                        tooltip: combatStore.RangedCooldownMinAndMax.Tooltip);
                    RangedSubListingHeight += Text.LineHeight + VerticalSpacing;
                    subListing.IntRange(ref combatStore.RangedCooldownMinAndMax.TempValue, 1, 200);
                    RangedSubListingHeight += IntSliderHeight;

                    CheckIntRange(ref combatStore.RangedCooldownMinAndMax.TempValue, 100);
                }
            }

            subListing.Gap();
            RangedSubListingHeight += 12f;
            listing.EndSection(subListing);
            listing.Gap();

            subListing = listing.SubListing(MeleeSubListingHeight);
            MeleeSubListingHeight = 0f;
            subListing.Gap();
            MeleeSubListingHeight += 12f;
            subListing.CheckboxLabeled(combatStore.MeleeHitEffectsEnabled.Label,
                ref combatStore.MeleeHitEffectsEnabled.TempValue, combatStore.MeleeHitEffectsEnabled.Tooltip);
            MeleeSubListingHeight += Text.LineHeight + VerticalSpacing;

            if (combatStore.MeleeHitEffectsEnabled.TempValue)
            {
                subListing.ShortGapLine();
                MeleeSubListingHeight += 12f;
                subListing.Label(combatStore.SurpriseAttackMultiplier.Label);
                MeleeSubListingHeight += Text.LineHeight + VerticalSpacing;
                rowRect = subListing.GetRect(IntSliderHeight);
                MeleeSubListingHeight += IntSliderHeight;
                var surpStr = combatStore.SurpriseAttackMultiplier.TempValue.IsTrivial()
                    ? "[Disabled]"
                    : $"x{combatStore.SurpriseAttackMultiplier.TempValue}";
                var surpNum = combatStore.SurpriseAttackMultiplier.TempValue;
                combatStore.SurpriseAttackMultiplier.TempValue = Widgets.HorizontalSlider(rowRect, surpNum, 0f, 2f,
                    false, surpStr, "Disabled", "x2", 0.1f);
                TooltipHandler.TipRegion(rowRect, combatStore.SurpriseAttackMultiplier.Tooltip);
                DrawIndicator(rowRect, 0.5f / 2f);

                subListing.ShortGapLine();
                MeleeSubListingHeight += 12f;
                subListing.Label(combatStore.DodgeCurviness.Label);
                MeleeSubListingHeight += Text.LineHeight + VerticalSpacing;
                rowRect = subListing.GetRect(IntSliderHeight);
                MeleeSubListingHeight += IntSliderHeight;
                var dodgeCurStr = combatStore.DodgeCurviness.TempValue.ToString();
                float dodgeCurNum = combatStore.DodgeCurviness.TempValue;
                combatStore.DodgeCurviness.TempValue = (int)Widgets.HorizontalSlider(rowRect, dodgeCurNum, 1f, 5f,
                    false, dodgeCurStr, "Flat", "Extreme Changes", 1f);
                TooltipHandler.TipRegion(rowRect, combatStore.DodgeCurviness.Tooltip);
                DrawIndicator(rowRect, 2f / 4);
            }

            subListing.Gap();
            MeleeSubListingHeight += 12f;
            listing.EndSection(subListing);
        }

        listing.End();
        Text.Anchor = anchor;
    }

    private void DrawIndicator(Rect rowRect, float fractionalPosition)
    {
        rowRect.x += texRextXMod + ((rowRect.width - (texRextXMod * 2)) * fractionalPosition) - 6f;
        rowRect.y += texRextYMod;
        rowRect.width = 12f;
        rowRect.height = 12f;
        Widgets.DrawTextureFitted(rowRect, IndicatorTex.DefIndicator, 1);
    }

    public void Clear()
    {
        bestAndWorstRangedCd = new string[2];
    }

    private void CheckIntRange(ref IntRange range, int mustInclude)
    {
        if (range.TrueMax != range.max)
        {
            var temp = range.max;
            range.max = range.TrueMax;
            range.min = temp;
        }

        if (range.min > mustInclude)
        {
            range.min = mustInclude;
        }

        if (range.max < mustInclude)
        {
            range.max = mustInclude;
        }
    }
}