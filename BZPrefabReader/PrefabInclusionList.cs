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
        "WorldEntities/EnvironmentProps/Expansion/TreeSpires/treespires_trees_01/ventgarden/Vent_garden_standing.prefab",
        "WorldEntities/EnvironmentProps/Expansion/TreeSpires/treespires_trees_01/ventgarden/VentGardenSmall.prefab",
    };
    
    private static readonly HashSet<string> Exclusions = new(StringComparer.OrdinalIgnoreCase)
    {
        "WorldEntities/Creatures/Expansion/IceWormSpawner.prefab",
        "WorldEntities/Creatures/Expansion/RockGrub.prefab",
    };
    
    public static bool ShouldSavePrefab(string path)
    {
        if (Exclusions.Contains(path))
        {
            return false;
        }

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