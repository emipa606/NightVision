# Copilot Instructions for NightVision (Continued)

## Mod Overview and Purpose
NightVision (Continued) is an enhancement and continuation of the original NightVision mod by drumads. Its core goal is to introduce mechanics that empower and challenge your colonists in dim light and dark environments, leveraging night vision and photosensitivity to alter gameplay dynamics significantly.

## Key Features and Systems
- **Night Vision Mechanics**: Eliminate slowdowns in darkness and negate negative thoughts associated with dim environments.
- **Photosensitivity Effects**: Faster operations in darkness, slower ones in bright light, with corresponding mood effects.
- **New Apparel and Bionics**: Includes items like night vision goggles and advanced helmets, along with bionic eyes providing night vision traits.
- **Alien Races Support**: Modify alien race traits to incorporate photosensitivity.
- **Customizable Settings**: Modify headgear effects and bionic implants to adjust night vision features.
- **Combat Enhancements**: Night vision and photosensitivity now affect hit chance, cooldowns, dodge chances, and can trigger sneak attacks.
- **Strategic Gameplay Changes**: Enemies prefer dark cover and have adjusted aggro ranges based on their vision capabilities.
- **New Raid Mechanic**: Introduces late-game night raids with solar flares affecting melee specialist encounters.

## Coding Patterns and Conventions
- **Class Naming**: Classes are generally named with a clear prefix indicating their purpose (e.g., `Comp_NightVision`, `NVGameComponent`). Utilize PascalCase for class names and methods.
- **Namespaces and Organisation**: Code is structured into static classes for utility (`CombatHelpers`) and derived classes for specific functionality (`Comp_NightVision`).
- **Methods**: Follow a consistent naming convention with verbs leading (e.g., `SetDirty`, `RemoveHediff`) using camelCase.

## XML Integration
- XML patches are utilized to integrate mod components with RimWorld's core game structures. Be aware of how XML definitions relate to in-game elements.
- Example patches are included in the mod for extending support to other mods or custom configurations.
- XML files are used to define items, their attributes, and how they interact with mod components.

## Harmony Patching
- **Harmony**: This mod integrates Harmony to patch RimWorld methods for expanded functionality.
- **Patch Locations**: Several static classes indicate where Harmony patches are applied (e.g., `PawnHealthTracker_AddHediff`, `StatPartGlow_FactorFromGlow`).

## Suggestions for Copilot
- **Code Completions**: Utilize Copilot for repetitive code patterns, especially in generating getters/setters or simple utility functions.
- **Harmony Integration**: Leverage Copilot to draft Harmony patching methods by identifying target method signatures in RimWorld's library.
- **XML Guidance**: Use Copilot to suggest XML configurations based on examples provided to ensure compatibility with RimWorld’s XML structure.
- **Refactor Suggestions**: Seek suggestions to ensure that repetitive logic is extracted into helper methods and constants are used to avoid magic numbers.

For further details on contributing or extending the mod, please consult the included source files and examples within the repository. Also, refer to the original mod documentation and any additional community guides for RimWorld modding best practices.
