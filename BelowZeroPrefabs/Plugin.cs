using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using BZtoSNLoader;
using ComponentPortFramework;
using HarmonyLib;
using Nautilus.Assets;
using Nautilus.Handlers;
using Newtonsoft.Json;

namespace BelowZeroPrefabs;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.snmodding.nautilus", "1.0.0.54")]
public class Plugin : BaseUnityPlugin
{
    public new static ManualLogSource Logger { get; private set; }

    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();

    internal static Dictionary<string, JsonPrefab> AssetPathsWithComponentData { get; private set; }
    
    private static ILogger ComponentSerializerLogger { get; } = new ComponentSerializerLogger();
    
    internal static PrefabComponentSerializer ComponentSerializer { get; } = new(ComponentSerializerLogger,
        new PrefabComponentSerializerSettings
        {
            EnableCreatureFixes = true,
            EnableAllComponents = true
        });

    private void Awake()
    {
        // set project-scoped logger instance
        Logger = base.Logger;
        
        Sn1TypeDatabase.Initialize(ComponentSerializerLogger);

        WaitScreenHandler.RegisterEarlyAsyncLoadTask("BelowZeroPrefabs", RegisterBzPrefabs, "Registering Below Zero prefabs");
        
        // register harmony patches, if there are any
        Harmony.CreateAndPatchAll(Assembly, $"{PluginInfo.PLUGIN_GUID}");
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }

    private void LoadComponentExportData()
    {
        var resourceName = "BelowZeroPrefabs.ComponentDataExport.json";

        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        using StreamReader reader = new StreamReader(stream);
        string result = reader.ReadToEnd();
        AssetPathsWithComponentData = JsonConvert.DeserializeObject<Dictionary<string, JsonPrefab>>(result);
    }

    private IEnumerator RegisterBzPrefabs(WaitScreenHandler.WaitScreenTask task)
    {
        LoadComponentExportData();
        yield return null;
        HashSet<string> used = new HashSet<string>();
        foreach (var asset in AssetPathsWithComponentData)
        {
            var classId = "BZ_" + asset.Key.Split('/').Last().Split('.').First();
            if (!used.Add(classId)) continue;
            new BzPrefab(PrefabInfo.WithTechType(classId), new BelowZeroAssetLoader.AddressablesAssetPath(asset.Key), asset.Value).Register();
        }
    }
}