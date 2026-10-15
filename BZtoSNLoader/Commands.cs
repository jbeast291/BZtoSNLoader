using System.Collections;
using JetBrains.Annotations;
using Nautilus.Commands;
using UnityEngine;
using UWE;

namespace BZtoSNLoader;

public static class Commands
{
    [UsedImplicitly]
    [ConsoleCommand("spawnbz")]
    private static void SpawnBzAsset(string prefabName, int count = 1, float distance = 12)
    {
        CoroutineHost.StartCoroutine(SpawnAsync(prefabName, count, distance));
    }
    
    private static IEnumerator SpawnAsync(string prefabName, int count, float distance)
    {
        TaskResult<GameObject> requestResult = new();
        yield return Plugin.AssetLoader.LoadAsset(prefabName, requestResult);
        GameObject result = requestResult.Get();
        if (result != null)
        {
            Debug.LogFormat("Spawning {0} {1}", count, distance);
            for (int i = 0; i < count; i++)
            {
                GameObject gameObject = Utils.CreatePrefab(result, distance, i > 0);
                LargeWorldEntity.Register(gameObject);
                gameObject.SendMessage("StartConstruction", SendMessageOptions.DontRequireReceiver);
            }
        }
        else
        {
            ErrorMessage.AddDebug("Could not find prefab by name: " + prefabName);
        }
    }
}