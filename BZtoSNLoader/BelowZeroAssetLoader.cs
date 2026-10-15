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
    public readonly record struct AddressablesAssetPath(string path);

    private BelowZeroGameData GameData { get; }
    private ManagedBundleLoader BundleLoader { get; }

    internal BelowZeroAssetLoader(BelowZeroGameData gameData, ManagedBundleLoader bundleLoader)
    {
        GameData = gameData;
        BundleLoader = bundleLoader;
    }

    public IEnumerator LoadAssetAsync<T>(AddressablesAssetPath asset, IOut<T> result) where T : Object
    {
        TaskResult<AssetBundle> bundleRequest = new TaskResult<AssetBundle>();
        yield return LoadAssetWithDependencies(asset, bundleRequest);

        AssetBundle bundle = bundleRequest.Get();

        int index = -1;
        string assetName = Path.GetFileName(asset.path);
        string[] allAssetsInBundle = bundle.GetAllAssetNames();
        for (int i = 0; i < allAssetsInBundle.Length; i++)
        {
            if (Path.GetFileName(allAssetsInBundle[i]) != assetName) continue;

            index = i;
            break;
        }

        if (index == -1)
        {
            throw new Exception($"Asset: {assetName} not present within requested asset bundle for {asset}");
        }

        result.Set((T)bundle.LoadAsset(allAssetsInBundle[index]));
    }

    private IEnumerator LoadAssetWithDependencies(AddressablesAssetPath asset, IOut<AssetBundle> assetBundle)
    {
        if (!GameData.BundleDependencyData.assetPathToBundleDeps.TryGetValue(asset.path, out List<string> bundles))
        {
            throw new InvalidKeyException($"Invalid BZ Addressables Path: \"{asset}\"!");
        }

        for (int i = bundles.Count - 1; i > 0; i--)
        {
            string bundlePath = GameData.GetPathToBelowZeroBundle(bundles[i]);
            if (!File.Exists(bundlePath)) 
            {
                throw new($"Required Dependency at: \"{bundlePath}\" not found!");
            }

            // TODO: More robust system needed for duplicate bundles between games
            if (bundles[i].Equals("e2e7857c51c8a8834e02aa4476c8baae_unitybuiltinshaders_44438d49f8b579f24000ed6aa7ed75f6.bundle")) 
            {
                continue;
            }

            yield return BundleLoader.RequestBundleAndAddUser(new(bundlePath), null);
        }

        string mainBundleWithAsset = GameData.GetPathToBelowZeroBundle(bundles[0]);
        yield return BundleLoader.RequestBundleAndAddUser(new(mainBundleWithAsset), assetBundle);
    }
}