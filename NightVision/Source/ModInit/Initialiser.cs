﻿// Nightvision NightVision Initialiser.cs
// 
// 03 08 2018
// 
// 16 10 2018

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using RimWorld;
using Verse;

namespace NightVision
{
    public static class Initialiser
    {
        public static void Startup()
        {
            FindSettingsDependentFields();

            FindDefsToAddNightVisionTo();
            Init_Research.AddNightVisionMarkerToVanillaResearch();
            Init_TapetumAnimals.AddTapetumRecipeToAnimals();


        }

        public static void FindDefsToAddNightVisionTo()
        {
            Init_Hediffs.FindAllValidHediffs();
            Init_Races.FindAllValidRaces();
            Init_Apparel.FindAllValidApparel();
        }

        
        public static void FindSettingsDependentFields()
        {
            FieldClearer.SettingsDependentFields = GenTypes.AllTypesWithAttribute<NVHasSettingsDependentFieldAttribute>().SelectMany(
                t => AccessTools.GetDeclaredFields(t).FindAll(fi => fi.HasAttribute<NVSettingsDependentFieldAttribute>())
            ).ToList();
        }
    }
}