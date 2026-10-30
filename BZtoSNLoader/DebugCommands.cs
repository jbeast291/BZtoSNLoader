using System.Collections;
using System.Collections.Generic;
using BepInEx;
using Nautilus.Commands;
using Nautilus.Utility;
using UnityEngine;
using UWE;
using Object = UnityEngine.Object;

namespace BZtoSNLoader;

internal static class DebugCommands
{
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

    private static readonly List<GalleryAsset> spawnedGalObjects = new();
    private static Coroutine activeSpawnLoop;

    [ConsoleCommand("bzgal")]
    private static void SpawnBzAsset(string subCommand = "", string filter = "")
    {
        switch (subCommand)
        {
            case "all":
                ResetActiveGallery();
                activeSpawnLoop = CoroutineHost.StartCoroutine(SpawnAllAsync(filter));
                ErrorMessage.AddDebug("Spawning every BZ asset (this will take a LONG time, the chain will be long)");
                break;
            case "reset": 
                ErrorMessage.AddDebug("Reset gallery");
                ResetActiveGallery();
                break;
            case "help":
                ErrorMessage.AddDebug(
                    "BtW's Assets Gallery Command Usage:" +
                    "\n<color=yellow>bzgal help</color> Shows this message." +
                    "\n<color=yellow>bzgal reset</color> Clears all spawned assets" +
                    "\n<color=yellow>bzgal all (filter)</color> Spawns all BZ assets (this will take a LONG time)"
                );
                break;
            default: 
                goto case "help";
        }
    }

    private static void ResetActiveGallery()
    {
        spawnedGalObjects.ForEach(p => Object.Destroy(p.gameObject));
        spawnedGalObjects.Clear();
        if(activeSpawnLoop != null)
        {
            CoroutineHost.StopCoroutine(activeSpawnLoop);
            activeSpawnLoop = null;
        }
    }

    private static IEnumerator SpawnAllAsync(string filter)
    {
        float nextX = 12;

        Vector3 startPosition = Player.main.transform.position;
        int total = BZGameData.bundleDependencyData.assetPathToBundleDeps.Keys.Count;
        int current = 0;

        GameObject cubeSpawner = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cubeSpawner.name = "BZCubeSpawner";
        Object.DestroyImmediate(cubeSpawner.GetComponent<BoxCollider>());
        cubeSpawner.GetComponent<MeshRenderer>().material = MaterialUtils.StasisFieldMaterial;
        GalleryAsset cubeGalAsset = cubeSpawner.AddComponent<GalleryAsset>();
        spawnedGalObjects.Add(cubeGalAsset);
        cubeSpawner.SetActive(true);
        cubeSpawner.transform.localScale = Vector3.one * 5;

        foreach (string prefabPath in BZGameData.bundleDependencyData.assetPathToBundleDeps.Keys)
        {
            current++;
            if(!prefabPath.EndsWith(".prefab")) continue;
            if(!filter.IsNullOrWhiteSpace() && !prefabPath.Contains(filter)) continue;

            Plugin.Logger.LogInfo($"Loading... {current}/{total} | {prefabPath}");

            TaskResult<GameObject> requestResult = new();
            yield return BZAssetLoader.LoadAssetAsync(new(prefabPath), requestResult);

            GameObject prefab = requestResult.Get();
            if(prefab == null) continue;

            GameObject bzPrefab = Object.Instantiate(prefab, startPosition + new Vector3(nextX, 0, 0), Quaternion.identity);
            bzPrefab.SetActive(true);
            GalleryAsset galAsset = bzPrefab.AddComponent<GalleryAsset>();
            galAsset.AddressablePath = prefabPath;
            spawnedGalObjects.Add(galAsset);

            float maxSize;
            {
                Bounds bounds;
                Renderer[] renderers = prefab.GetAllComponentsInChildren<Renderer>();
                if (renderers.Length > 0) {
                    bounds = renderers[0].bounds;
                    foreach (Renderer renderer in renderers)
                    {
                        bounds.Encapsulate(renderer.bounds);
                    }
                }
                else bounds = new(Vector3.zero, Vector3.zero);

                maxSize = bounds.size.x;
                maxSize = maxSize.Clamp(0, 50);
            }

            nextX += maxSize;
            cubeSpawner.transform.position = startPosition + new Vector3(nextX, 0, 0);
        }
    }

    private class GalleryAsset() : MonoBehaviour { internal string AddressablePath; }
}