using System;
using System.Collections.Generic;

namespace BZPrefabReader;

public static class PrefabInclusionList
{
    private static readonly string[] Folders =
    {
        "WorldEntities/Creatures/Expansion"
    };

    private static readonly HashSet<string> SpecialInclusions = new(StringComparer.OrdinalIgnoreCase)
    {
        // Creatures
        "WorldEntities/EnvironmentProps/Expansion/TreeSpires/treespires_trees_01/ventgarden/Vent_garden_standing.prefab",
        "WorldEntities/EnvironmentProps/Expansion/TreeSpires/treespires_trees_01/ventgarden/VentGardenSmall.prefab",

        // Other cool things
        "WorldEntities/Tools/Hoverbike.prefab",
        "WorldEntities/Tools/Hovercraft_remote.prefab",
        "WorldEntities/Tools/MetalDetector.prefab",
        "WorldEntities/Tools/QuantumLocker.prefab",
        "Submarine/Build/BaseStatus.prefab",
        "Submarine/Build/Fridge.prefab",
        "Submarine/Build/Jukebox.prefab",
        "Submarine/Build/PhaseGate.prefab",
        "Submarine/Build/Recyclotron.prefab",
        "Submarine/Build/Shower.prefab",
        "Submarine/Build/Sink.prefab",
        "Submarine/Build/SmallLocker.prefab",
        "Submarine/Build/SmallStove.prefab"
    };
    
    public static bool ShouldSavePrefab(string path)
    {
        foreach (var folder in Folders)
        {
            if (path.StartsWith(folder, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return SpecialInclusions.Contains(path);
    }
}