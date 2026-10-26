using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Nautilus.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace BZtoSNLoader;

/// <summary>
/// The primary API for loading Below Zero assets by their address.
/// </summary>
public static class BZAssetLoader
{
    /// <summary>
    /// Holds a path to an Addressable asset.
    /// </summary>
    /// <param name="Path">The full path including the file extension.</param>
    /// <example><c>WorldEntities/Alterra/AbandonedBases/AlterraBase2/Alterra_Base_2.prefab</c></example>
    public readonly record struct AddressablesAssetPath(string Path);

    /// <summary>
    /// Asynchronously loads the given asset by its path and outputs the loaded asset to the given <paramref name="result"/> object.
    /// </summary>
    /// <remarks>
    /// This method handles automatic loading of dependencies. However, it does not handle the unloading of assets when they become unused.
    /// </remarks>
    /// <param name="asset">The path of the addressable asset.</param>
    /// <param name="result">The interface used to output the result. See <seealso cref="TaskResult{T}"/>.</param>
    /// <typeparam name="T">The type of asset to load. Many Unity types are supported such as <c>GameObject</c>,
    /// <c>Sprite</c>, <c>Texture2D</c>, etc.</typeparam>
    /// <returns></returns>
    /// <exception cref="Exception">This method throws an exception if 1) the Below Zero installation was not found or 2)
    /// the asset does not exist in any bundle.</exception>
    /// <example>
    /// <code>
    /// TaskResult&lt;GameObject&gt; result = new TaskResult&lt;GameObject&gt;();
    /// yield return LoadAssetAsync(new AddressablesAssetPath("WorldEntities/Creatures/Expansion/ArcticPeeper.prefab"), result);
    /// GameObject prefab = result.Get();
    /// </code></example>
    public static IEnumerator LoadAssetAsync<T>(AddressablesAssetPath asset, IOut<T> result) where T : UnityEngine.Object
    {
        if (!BZGameData.BelowZeroFound)
        {
            throw new Exception("BZ Asset Loader not initialized! Check the console for previous path errors");
        }
        TaskResult<AssetBundle> bundleRequest = new();
        yield return LoadAssetWithDependencies(asset, bundleRequest);

        AssetBundle bundle = bundleRequest.Get();
        if (bundle == null)
        {
            result.Set(null);
            Plugin.Logger.LogError($"{asset} already loaded externally! The base game has this asset already loaded, there is no reason to pull it from BZ!");
            yield break;
        }
        int index = -1;
        string assetName = Path.GetFileName(asset.Path);
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

    private static IEnumerator LoadAssetWithDependencies(AddressablesAssetPath asset, IOut<AssetBundle> assetBundle)
    {
        if (!BZGameData.BundleDependencyData.assetPathToBundleDeps.TryGetValue(asset.Path, out List<string> bundles))
        {
            throw new InvalidKeyException($"Invalid BZ Addressables Path: \"{asset}\"!");
        }

        for (int i = bundles.Count - 1; i > 0; i--)
        {
            string bundlePath = BZGameData.GetPathToBelowZeroBundle(bundles[i]);
            if (!File.Exists(bundlePath)) 
            {
                throw new($"Required Dependency at: \"{bundlePath}\" not found!");
            }

            // TODO: More robust system needed for duplicate bundles between games
            if (bundles[i].Equals("e2e7857c51c8a8834e02aa4476c8baae_unitybuiltinshaders_44438d49f8b579f24000ed6aa7ed75f6.bundle")) 
            {
                continue;
            }

            yield return ManagedBundleLoader.RequestBundleAndAddUser(new(bundlePath), null);
        }

        string mainBundleWithAsset = BZGameData.GetPathToBelowZeroBundle(bundles[0]);
        yield return ManagedBundleLoader.RequestBundleAndAddUser(new(mainBundleWithAsset), assetBundle);
    }
}