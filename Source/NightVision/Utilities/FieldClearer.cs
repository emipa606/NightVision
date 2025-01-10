using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace NightVision;

public static class FieldClearer
{
    public static List<Traverse> SettingsDependentFieldTraverses = [];

    public static void FindSettingsDependentFields()
    {
        var traverses = new List<Traverse>();

        var markedTypes = GenTypes.AllTypesWithAttribute<NVHasSettingsDependentFieldAttribute>();

        foreach (var type in markedTypes)
        {
            var fields = AccessTools.GetDeclaredFields(type)
                .FindAll(fi => fi.HasAttribute<NVSettingsDependentFieldAttribute>());

            foreach (var info in fields)
            {
                var traverse = new Traverse(type);
                traverse = traverse.Field(info.Name);

                traverses.Add(traverse);
            }
        }


        SettingsDependentFieldTraverses = traverses;
    }


    public static void ResetSettingsDependentFields()
    {
        try
        {
            if (SettingsDependentFieldTraverses.Count == 0)
            {
                return;
            }

            foreach (var fieldTraverse in SettingsDependentFieldTraverses)
            {
                if (!fieldTraverse.FieldExists())
                {
                    Log.Warning("SettingsDependentFieldTraverses included a field that did not exist.");
                    continue;
                }

                var type = fieldTraverse.GetValueType();

                if (type == typeof(TriBool))
                {
                    fieldTraverse.SetValue(TriBool.Undefined);
                }
                else if (type == typeof(int))
                {
                    fieldTraverse.SetValue(-9999);
                }
                else if (type == typeof(float))
                {
                    fieldTraverse.SetValue(-9999f);
                }
                else if (type.IsClass)
                {
                    fieldTraverse.SetValue(null);
                }
                else
                {
                    Log.Warning(
                        $"FieldClearer: unsupported settings type. {fieldTraverse.GetValueType()}, {fieldTraverse.GetValue()}");
                }
            }
        }
        catch
        {
            // ignored
        }
    }
}