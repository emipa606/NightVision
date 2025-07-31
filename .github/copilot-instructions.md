# GitHub Copilot Instructions for RimWorld Modding Project

## Mod Overview and Purpose
This RimWorld mod is designed to enhance and tweak various aspects of AI combat behavior and vision mechanics within the game. It introduces nuanced systems for night vision and modifies AI combat routines to create a more dynamic gameplay experience. The mod leverages C# for most of its logic, while extensive use of XML facilitates integration within the RimWorld framework.

## Key Features and Systems
- **AI Combat Enhancements**: Modifications to AI combat behaviors to improve tactical decision-making.
- **Night Vision System**: Introduces night vision capabilities, affecting both pawn perception and apparel properties.
- **Custom Light Modifiers**: Provides mechanisms to adjust light modifiers that affect game mechanics like sight and combat effectiveness.
- **Solar Raid Events**: Special raid mechanics and events tied to solar activity.
- **Comprehensive Apparel and Hediff System**: Dynamic handling of apparel and health conditions (hediffs) that influence gameplay.

## Coding Patterns and Conventions
- **Static Helper Classes**: Used extensively for utility functions (e.g., `CombatHelpers`, `PawnGenUtility`).
- **Partial Classes for Initialization**: The `Initialiser` class is split into partial classes for modular initialization of various game aspects like hediffs, apparel, and races.
- **Use of Interfaces**: Interfaces like `IExposable` and `ISaveCheck` ensure proper data exposure and save-check functionality.

## XML Integration
- **Defs Integration**: The mod utilizes XML for defining various game elements and integrating them into RimWorld. This includes configurations for night vision and other modified elements.
- **Mod Extensions**: Custom XML definitions extend standard game mechanics, allowing for seamless integration of new features.

## Harmony Patching
- The mod uses [Harmony](https://harmony.pardeike.net/) for runtime method patching to alter game behavior without directly modifying the base game code.
- **Harmony Setup**: The `NVHarmonyPatcher` class is responsible for setting up all Harmony patches used by the mod.
- **Patch Locations**: Specific methods related to AI and combat are targeted for patching to refine gameplay dynamics.

## Suggestions for Copilot
- **Generate Utility Functions**: Copilot can assist in writing utility functions by detecting patterns in existing helper classes and extending their capabilities.
- **Create XML Templates**: Automatic generation of XML templates for new defs can streamline the integration of additional features.
- **Streamline Initialization Routines**: Propose improvements to static initialization routines to enhance modularity and maintainability.
- **Harmony Patch Design**: Suggest potential Harmony patches by analyzing current method usages and proposing alternative or optimized implementations.
- **Dynamic Gameplay Adjustments**: Recommend gameplay adjustments based on observed interactions and existing modifiers.

This document serves as a guide for utilizing GitHub Copilot efficiently in the development process of this RimWorld mod. It highlights the core aspects of the mod and provides directions for enhancing development workflows using AI-assisted capabilities.
