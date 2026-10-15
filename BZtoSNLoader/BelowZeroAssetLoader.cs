using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace BZtoSNLoader;

public class BelowZeroAssetLoader
{
    private BelowZeroGameData GameData { get; }
    private ManagedBundleLoader BundleLoader { get; }
    
    internal BelowZeroAssetLoader(BelowZeroGameData gameData, ManagedBundleLoader bundleLoader)
    {
        GameData = gameData;
        BundleLoader = bundleLoader;
    }
    
    public IEnumerator LoadAsset<T>(string assetPath, IOut<T> result) where T : Object
    {
        TaskResult<AssetBundle> bundle = new TaskResult<AssetBundle>();
        yield return LoadAssetDependencies(assetPath, bundle);
        var asset = bundle.Get().LoadAllAssets()[0];
        result.Set((T)asset);
    }

    private IEnumerator LoadAssetDependencies(string assetPath, IOut<AssetBundle> assetBundle)
    {
        if (!GameData.BundleDependencyData.assetPathToBundleDeps.TryGetValue(assetPath, out List<string> bundles))
        {
            throw new InvalidKeyException($"Invalid BZ Addressables Path: \"{assetPath}\"!");
        }

        for (int i = bundles.Count - 1; i > 0; i--)
        {
            string bundlePath = GameData.GetPathToBelowZeroBundle(bundles[i]);
            if (!File.Exists(bundlePath)) 
            {
                throw new Exception($"Required Dependency at: \"{bundlePath}\" not found!");
            }
            yield return BundleLoader.RequestBundleAndAddUser(assetPath);
        }
        
        string bundleWithAsset = GameData.GetPathToBelowZeroBundle(bundles[0]);
        yield return BundleLoader.RequestBundleAndAddUser(bundleWithAsset, assetBundle);
    }
}