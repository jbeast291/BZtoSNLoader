using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using DependencyDataFormat;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace BZtoSNLoader;


[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
internal class Plugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger { get; private set; }
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();

    private const string dependencyDataFile = "assetPathToBundleDeps.json";
    private static BundleDependencyFormat dependencyData;

    private void Awake()
    {
        Logger = base.Logger;
        LoadDependencyJson();
    }

    private static void LoadDependencyJson() 
    {
        string jsonData = File.ReadAllText(GetPathFor(dependencyDataFile));
        dependencyData = JsonConvert.DeserializeObject<BundleDependencyFormat>(jsonData);

        if (dependencyData.Version != BundleDependencyFormat.latestVersion)
        {
            Logger.LogError($"Invalid Dependency Version! Please ensure you replaced the \"{dependencyDataFile}\" when updating!");
        }
    }

    private void CreatePrefabAtPlayer(string assetPath)
    {
        GameObject prefab = (GameObject) LoadPrefab(assetPath);
        GameObject go = Instantiate(prefab);
        go.transform.position = Player.main.transform.position;
    }

    private UnityEngine.Object LoadPrefab(string assetPath)
    {
        if(!dependencyData.assetPathToBundleDeps.TryGetValue(assetPath, out List<string> bundles))
        {
            throw new InvalidKeyException($"Invalid BZ Addresables Path: \"{assetPath}\"!");
        }

        for(int i = bundles.Count - 1; i > 0; i--)
        {
            string bundlePath = GetPathToBZBundle(bundles[i]);
            if (!File.Exists(bundlePath)) 
            {
                throw new Exception($"Required Dependency at: \"{bundlePath}\" not found!");
            }
            AssetBundle.LoadFromFile(bundlePath);
        }

        string bundleWithAsset = GetPathToBZBundle(bundles[0]);
        var gos = AssetBundle.LoadFromFile(bundleWithAsset).LoadAllAssets();
        return gos[0];
    }

    private const string pathToBZAddressablesFolder = @"Z:\home\jbeast\.local\share\Steam\steamapps\common\SubnauticaZero\";

    private static readonly string bzAddressableFolder 
        = Path.Combine(pathToBZAddressablesFolder, "SubnauticaZero_Data", "StreamingAssets", "aa", "StandaloneWindows64/");
    
    private static string GetPathToBZBundle(string bundleName) 
        => Path.Combine(bzAddressableFolder, bundleName);

    private static string GetPathFor(string assetName) 
        => Path.Combine(Path.GetDirectoryName(Assembly.Location)!, assetName);
} 