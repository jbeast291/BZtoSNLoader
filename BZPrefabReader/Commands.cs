using System.Collections;
using System.IO;
using System.Net;
using Nautilus.Commands;
using Newtonsoft.Json;
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

        var task = CraftData.GetPrefabForTechTypeAsync(TechType.LilyPadStage2);
        yield return task;

        ErrorMessage.AddMessage("loaded lily pad");
        
        var result = task.GetResult();
        var prefab = PrefabReaderUtils.ReadPrefab(result);
        File.WriteAllText("READ-PREFAB.txt", JsonConvert.SerializeObject(prefab));
        var reconstructed = PrefabReaderUtils.ReconstructPrefab(prefab);
        
        ErrorMessage.AddMessage("Done reading prefabs...");
    }
}