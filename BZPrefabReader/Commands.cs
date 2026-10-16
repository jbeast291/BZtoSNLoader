using System.Collections;
using System.IO;
using Nautilus.Commands;
using Newtonsoft.Json;
using UnityEngine;
using UWE;

namespace BZPrefabReader;

public static class Commands
{
    [ConsoleCommand("spawntest")]
    private static void ReadPrefabs(string techType)
    {
        CoroutineHost.StartCoroutine(SpawnAsync(techType));
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
        var prefabData = PrefabReaderUtils.ReadPrefab(prefab);
        File.WriteAllText("READ-PREFAB.txt", JsonConvert.SerializeObject(prefabData));

        var arcticPeeper = UWE.Utils.InstantiateDeactivated(prefab, Vector3.zero, Quaternion.identity);
        ComponentStripUtils.StripComponents(arcticPeeper);
        
        ErrorMessage.AddMessage("Stripped components");
        
        PrefabReaderUtils.ReconstructComponents(arcticPeeper, prefabData);
        arcticPeeper.SetActive(true);
        arcticPeeper.transform.position = Player.main.transform.position;
        
        ErrorMessage.AddMessage("Reconstructed prefab");
        
        // ErrorMessage.AddMessage("Done reading prefabs...");
    }
}