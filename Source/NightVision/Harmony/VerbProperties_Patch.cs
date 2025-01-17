﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace NightVision.Harmony;

[HarmonyPatch(typeof(VerbProperties), nameof(VerbProperties.AdjustedCooldown), typeof(Tool), typeof(Pawn),
    typeof(Thing))]
[UsedImplicitly]
public static class VerbProperties_AdjustedCooldown

{
    [UsedImplicitly]
    public static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions
    )
    {
        var instructionsList = instructions.ToList();
        var signifyingMethod = AccessTools.Method(typeof(StatExtension), nameof(StatExtension.GetStatValue));

        foreach (var current in instructionsList)
        {
            yield return current;
            if (!current.Is(OpCodes.Call, signifyingMethod))
            {
                continue;
            }

            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(CombatHelpers), nameof(CombatHelpers.AdjustCooldownForGlow)));
        }
    }
}