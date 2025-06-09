// Nightvision CastPositionFinder_CastPositionPreference.cs
// 
// 20 10 2018
// 
// 20 10 2018

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace NightVision.Harmony;

[HarmonyPatch(typeof(CastPositionFinder), "CastPositionPreference")]
public static class CastPositionFinder_CastPositionPreference
{
    private static readonly float GlowCoverCoefficient = 0.5f;


    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var instrList = instructions.ToList();

        CodeInstruction loadIntVecArg = null;
        CodeInstruction loadCastPositionReq = null;
        CodeInstruction loadCasterPawnFromReq = null;

        var callOurMethod = new CodeInstruction(
            OpCodes.Call,
            AccessTools.Method(typeof(CastPositionFinder_CastPositionPreference), nameof(ModifyCoverDesirability))
        );

        var workDone = false;

        for (var index = 0; index < instrList.Count; index++)
        {
            var instr = instrList[index];

            yield return instr;

            if (workDone)
            {
                continue;
            }

            if (instr.opcode == OpCodes.Ldarg_0)
            {
                loadIntVecArg = instr;
            }
            else if (instr.opcode == OpCodes.Ldsflda && instr.operand is FieldInfo fi &&
                     fi.FieldType == typeof(CastPositionRequest)
                     && instrList[index + 1].opcode == OpCodes.Ldfld &&
                     instrList[index + 1].operand is FieldInfo fi2 && fi2.FieldType == typeof(Pawn))
            {
                loadCastPositionReq = new CodeInstruction(instr);
                loadCasterPawnFromReq = new CodeInstruction(instrList[index + 1]);
            }

            else if (instr.opcode == OpCodes.Mul
                     && instrList[index - 1].opcode == OpCodes.Ldc_R4
                     && instrList[index + 1].opcode == OpCodes.Add)
            {
                yield return loadIntVecArg;
                yield return loadCastPositionReq;
                yield return loadCasterPawnFromReq;
                yield return callOurMethod;
                yield return new CodeInstruction(instr);

                workDone = true;
            }
        }
    }


    public static float ModifyCoverDesirability(IntVec3 c, Pawn caster)
    {
        var glow = GlowFor.GlowAt(caster.Map, c);

        if (glow.GlowIsDarkness())
        {
            return 1f + (GlowCoverCoefficient * ((Constants.MIN_GLOW_NO_GLOW - glow) / Constants.MIN_GLOW_NO_GLOW));
        }

        return 1f;
    }
}