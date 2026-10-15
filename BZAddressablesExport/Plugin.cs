using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using DependencyDataFormat;
using Newtonsoft.Json;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace BZAddressablesExport;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private new static ManualLogSource Logger { get; set; }

    private IEnumerator Start()
    {
        Logger = base.Logger;
        yield return Addressables.InitializeAsync();
        DumpDependencies();
    }

    private static string GetPathFor(string assetName) 
        => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, assetName);

    private static void DumpDependencies()
    {
        BundleDependencyFormat format = DumpAllAssetBundleDependencies();
        string jsonText = JsonConvert.SerializeObject(format, Formatting.Indented);
        File.WriteAllText(GetPathFor("assetPathToBundleDeps.json"), jsonText);
    }

    private static BundleDependencyFormat DumpAllAssetBundleDependencies()
    {
        BundleDependencyFormat format = new();

        foreach (IResourceLocator locator in Addressables.ResourceLocators)
        {
            foreach (object key in locator.Keys)
            {
                if (key is not string keyString) continue;

                if (keyString.Contains(".bundle") || !keyString.Contains(".")) continue;

                format.assetPathToBundleDeps.Add(keyString, GetDependenciesFor(locator, keyString));
            }
        }
        return format;
    }

    private static List<string> GetDependenciesFor(IResourceLocator locator, string key)
    {
        List<string> dependencies = new();

        if (!locator.Locate(key, typeof(object), out IList<IResourceLocation> locations) || locations == null) return dependencies; 

        foreach (IResourceLocation location in locations)
        {
            if (location?.Dependencies == null) continue;

            foreach (IResourceLocation dependency in location.Dependencies)
            {
                if (dependency == null) continue;
                string depId = dependency.InternalId ?? string.Empty;
                string fileName = Path.GetFileName(depId);

                if (!string.IsNullOrEmpty(fileName) && !dependencies.Contains(fileName)) 
                {
                    dependencies.Add(fileName);
                }
            }
        }

        return dependencies;
    }
}