using System;
using System.Collections.Generic;
using System.Linq;

namespace BelowZeroCreatures.CreatureModifiers.System;

internal static class CreatureModifiersManager
{
    private static Dictionary<string, BzCreatureModifier> Modifiers { get; set; }
    
    internal static void RegisterCreatureEdits()
    {
        Modifiers = new Dictionary<string, BzCreatureModifier>(StringComparer.OrdinalIgnoreCase);
        var types = Plugin.Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(BzCreatureModifier)));
        foreach (var type in types)
        {
            var instance = (BzCreatureModifier)Activator.CreateInstance(type);
            foreach (var classId in instance.ClassIds)
            {
                Modifiers.Add(classId, instance);
            }
        }
    }

    public static bool TryGetCreatureEdit(string classId, out BzCreatureModifier modifier)
    {
        return Modifiers.TryGetValue(classId, out modifier);
    }

    internal static void CheckForInvalidModifiers(HashSet<string> allValidPrefabClassIDs)
    {
        foreach (var modifier in Modifiers)
        {
            if (!allValidPrefabClassIDs.Contains(modifier.Key))
            {
                throw new Exception($"Creature modifier references unknown ClassID '{modifier.Key}'");
            }
        }
    }
}