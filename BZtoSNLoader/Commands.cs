using System.Collections;
using JetBrains.Annotations;
using Nautilus.Commands;
using UnityEngine;
using UWE;

namespace BZtoSNLoader;

internal static class Commands
{
    [UsedImplicitly]
    [ConsoleCommand("spawnbz")]
    private static void SpawnBzAsset(string prefabName, int count = 1, float distance = 12)
    {
        CoroutineHost.StartCoroutine(SpawnAsync(prefabName, count, distance));
    }

    private static IEnumerator SpawnAsync(string assetPath, int count, float distance)
    {
        TaskResult<GameObject> requestResult = new();
        yield return Plugin.AssetLoader.LoadAssetAsync(new(assetPath), requestResult);
        GameObject result = requestResult.Get();
        if (result != null)
        {
            Debug.LogFormat("Spawning {0} {1}", count, distance);
            for (int i = 0; i < count; i++)
            {
                Utils.CreatePrefab(result, distance, i > 0);
            }
        }
        else
        {
            ErrorMessage.AddDebug("Could not find prefab by name: " + assetPath);
        }
    }
}