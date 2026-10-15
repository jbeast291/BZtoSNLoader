using System.Collections;
using System.IO;
using System.Net;
using Nautilus.Commands;
using Newtonsoft.Json;
using UnityEngine;
using UWE;

namespace BZPrefabReader;

public static class Commands
{
    [ConsoleCommand("readallprefabs")]
    private static void ReadPrefabs()
    {
        CoroutineHost.StartCoroutine(ReadPrefabsAsync());
    }

    private static IEnumerator ReadPrefabsAsync()
    {
        ErrorMessage.AddMessage("Reading prefabs...");

        var task = CraftData.GetPrefabForTechTypeAsync(TechType.ArcticPeeper);
        yield return task;

        ErrorMessage.AddMessage("Loaded arctic peeper");
        
        var prefab = task.GetResult();
        var prefabData = PrefabReaderUtils.ReadPrefab(prefab);
        File.WriteAllText("READ-PREFAB.txt", JsonConvert.SerializeObject(prefabData));

        var arcticPeeper = UWE.Utils.InstantiateDeactivated(prefab, Vector3.zero, Quaternion.identity);
        ComponentStripUtils.StripComponents(arcticPeeper);
        
        ErrorMessage.AddMessage("Stripped components from Arctic Peeper");
        
        PrefabReaderUtils.ReconstructComponents(arcticPeeper, prefabData);
        arcticPeeper.SetActive(true);
        
        ErrorMessage.AddMessage("Reconstructed Arctic Peeper");
        
        // ErrorMessage.AddMessage("Done reading prefabs...");
    }
}