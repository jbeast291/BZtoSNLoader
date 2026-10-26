using System;
using System.Collections;
using JetBrains.Annotations;
using Nautilus.Commands;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using UWE;
using Object = UnityEngine.Object;

namespace BZtoSNLoader;

internal static class DebugCommands
{
    [UsedImplicitly]
    [ConsoleCommand("spawnbz")]
    private static void SpawnBzAsset(string prefabName, int count = 1, float distance = 12) 
        => CoroutineHost.StartCoroutine(SpawnAsync(prefabName, count, distance));

    private static IEnumerator SpawnAsync(string assetPath, int count, float distance)
    {
        TaskResult<GameObject> requestResult = new();
        yield return BZAssetLoader.LoadAssetAsync(new(assetPath), requestResult);
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
    
    [UsedImplicitly]
    [ConsoleCommand("spawnallbz")]
    private static void SpawnBzAsset() => CoroutineHost.StartCoroutine(SpawnAllAsync());

    private static IEnumerator SpawnAllAsync()
    {
        float rowX = 12;

        Vector3 startPosition = Player.main.transform.position;
        int total = BZGameData.BundleDependencyData.assetPathToBundleDeps.Keys.Count;
        int current = 0;

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.SetActive(true);
        cube.transform.localScale = Vector3.one * 5;

        foreach (string prefabPath in BZGameData.BundleDependencyData.assetPathToBundleDeps.Keys)
        {
            current++;
            if(!prefabPath.EndsWith(".prefab") && 
               !prefabPath.EndsWith(".FBX") && 
               !prefabPath.EndsWith(".fbx")
            ) {
                continue;
            }
            Plugin.Logger.LogError($"Loading: {current}/{total}");

            TaskResult<GameObject> requestResult = new();
            yield return BZAssetLoader.LoadAssetAsync(new(prefabPath), requestResult);

            GameObject prefab = requestResult.Get();
            if(prefab == null) continue;

            GameObject gameObject = Object.Instantiate(prefab, startPosition + new Vector3(rowX, 0, 0), Quaternion.identity);
            gameObject.SetActive(true);

            Renderer[] renderers = prefab.GetAllComponentsInChildren<Renderer>();
            Bounds bounds;
            if (renderers.Length > 0) {
                bounds = renderers[0].bounds;
                foreach (Renderer renderer in renderers)
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }
            else bounds = new(Vector3.zero, Vector3.zero);

            float maxSize = bounds.size.x;
            maxSize = maxSize.Clamp(0, 50);

            rowX += maxSize;
            cube.transform.position = startPosition + new Vector3(rowX, 0, 0);
        }
    }
}