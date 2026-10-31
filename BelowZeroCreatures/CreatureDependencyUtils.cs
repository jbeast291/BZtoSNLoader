using System.Collections;
using UnityEngine;

namespace BelowZeroCreatures;

internal static class CreatureDependencyUtils
{
    private static GameObject _creatureRespawnerPrefab;
    private static GameObject _bloodParticlePrefab;

    public static IEnumerator GetCreatureRespawnerPrefab(IOut<GameObject> result)
    {
        // Check if prefab is already cached
        if (_creatureRespawnerPrefab != null)
        {
            result.Set(_creatureRespawnerPrefab);
            yield break;
        }
        
        // Fetch instance of prefab
        var request = CraftData.GetPrefabForTechTypeAsync(TechType.Peeper);
        yield return request;
        var peeper = request.GetResult();
        if (peeper == null)
        {
            Plugin.Logger.LogError("Peeper prefab not found");
            yield break;
        }

        if (!peeper.TryGetComponent<CreatureDeath>(out var death))
        {
            Plugin.Logger.LogError("CreatureDeath component not found on Peeper - this is unusual");
            yield break;
        }

        var respawner = death.respawnerPrefab;
        if (respawner == null)
        {
            Plugin.Logger.LogError($"No valid creature respawner prefab found on {death}");
            yield break;
        }

        _creatureRespawnerPrefab = respawner;
        result.Set(respawner);
    }
    
    public static IEnumerator GetBloodParticlePrefab(IOut<GameObject> result)
    {
        // Check if prefab is already cached
        if (_bloodParticlePrefab != null)
        {
            result.Set(_bloodParticlePrefab);
            yield break;
        }
        
        // Fetch instance of prefab
        var request = CraftData.GetPrefabForTechTypeAsync(TechType.Peeper);
        yield return request;
        var peeper = request.GetResult();
        if (peeper == null)
        {
            Plugin.Logger.LogError("Peeper prefab not found");
            yield break;
        }

        var liveMixin = peeper.GetComponent<LiveMixin>();
        if (liveMixin == null || liveMixin.data == null)
        {
            Plugin.Logger.LogError($"No valid LiveMixin data found on {peeper}");
            yield break;
        }

        _bloodParticlePrefab = liveMixin.data.damageEffect;
        result.Set(_bloodParticlePrefab);
    }
}