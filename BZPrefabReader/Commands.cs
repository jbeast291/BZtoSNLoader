using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ComponentPortFramework;
using Nautilus.Commands;
using Newtonsoft.Json;
using UnityEngine;
using UWE;

namespace BZPrefabReader;

public static class Commands
{
    private static readonly PrefabComponentSerializer Serializer = new(Plugin.FrameworkLogger,
        new PrefabComponentSerializerSettings
        {
            EnableCreatureFixes = true,
            EnableAllComponents = true
        });
    
    [ConsoleCommand("spawntest")]
    private static void SpawnTest(string techType)
    {
        CoroutineHost.StartCoroutine(SpawnAsync(techType));
    }
    
    [ConsoleCommand("readallprefabs")]
    private static void ReadAllPrefabs()
    {
        CoroutineHost.StartCoroutine(ReadAllAsync());
    }

    private static IEnumerator SpawnAsync(string prefabName)
    {
        ErrorMessage.AddMessage("Reading prefab...");

        if (!TechTypeExtensions.FromString(prefabName, out var techType, true))
        {
            ErrorMessage.AddMessage("Could not find TechType by name " + prefabName);
            yield break;
        }
        
        var task = CraftData.GetPrefabForTechTypeAsync(techType);
        yield return task;

        ErrorMessage.AddMessage("Loaded " + techType);
        
        var prefab = task.GetResult();
        var prefabData = Serializer.SavePrefabComponents(prefab);

        var arcticPeeper = UWE.Utils.InstantiateDeactivated(prefab, Vector3.zero, Quaternion.identity);
        ComponentStripUtils.StripComponents(arcticPeeper);
        
        ErrorMessage.AddMessage("Stripped components");
        
        Serializer.ReconstructComponentsOnGameObject(arcticPeeper, prefabData);
        arcticPeeper.SetActive(true);
        arcticPeeper.transform.position = Player.main.transform.position;
        
        ErrorMessage.AddMessage("Reconstructed prefab");
    }

    private static IEnumerator ReadAllAsync()
    {
        var outputDir = "PrefabExports";
        if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);
        var files = PrefabDatabase.prefabFiles;
        Dictionary<string, JsonPrefab> master = new Dictionary<string, JsonPrefab>();
        foreach (var file in files)
        {
            // check
            if (!ShouldSavePrefab(file.Value)) continue;
            
            // load
            var request = PrefabDatabase.GetPrefabAsync(file.Key);
            yield return request;
            if (!request.TryGetPrefab(out var prefab))
            {
                ErrorMessage.AddMessage("FAILED to load prefab at path " + file.Value);
                continue;
            }
            
            // serialize & output
            try
            {
                var prefabData = Serializer.SavePrefabComponents(prefab);
                master.Add(file.Value, prefabData);
            }
            catch (Exception e)
            {
                ErrorMessage.AddMessage("Error while loading prefab " + file.Value);
                Plugin.Logger.LogError(e);
            }
        }
        var outputPath = Path.Combine(outputDir, "ComponentDataExport" + ".json");
        File.WriteAllText(outputPath, JsonConvert.SerializeObject(master));
        ErrorMessage.AddMessage("Saved data to BZ game folder");
    }

    private static bool ShouldSavePrefab(string path)
    {
        if (path.StartsWith("WorldEntities/Creatures")) return true;
        if (path.StartsWith("Submarine/Build")) return true;
        if (path.StartsWith("WorldEntities/Tools")) return true;
        if (path.EndsWith("Vent_garden_standing.prefab")) return true;
        if (path.EndsWith("VentGardenSmall.prefab")) return true;
        return false;
    }
}