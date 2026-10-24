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