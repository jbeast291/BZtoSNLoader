using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using UnityEngine;

namespace BZtoSNLoader;


// This class is not safe to expose. You must request and release for users of a bundle.
internal static class ManagedBundleLoader
{
    internal readonly record struct BundleFullPath(string location);

    private static readonly Dictionary<string, BundleInstance> _bundles = new(StringComparer.OrdinalIgnoreCase);

    internal static IEnumerator RequestBundleAndAddUser(BundleFullPath bundlePath, [CanBeNull] IOut<AssetBundle> result)
    {
        string id = GetIdentifierFromBundlePath(bundlePath);

        if (!_bundles.TryGetValue(id, out var bundle))
        {
            bundle = new();
            _bundles.Add(id, bundle);
        }

        bundle.Users++;

        bool wasAlreadyLoaded;
        switch (bundle.State)
        {
            // Load the bundle
            case BundleState.Unloaded: wasAlreadyLoaded = false; break;
            // Wait for the bundle to finish loading
            case BundleState.Loading: yield return new WaitUntil(() => bundle.State == BundleState.Loaded); goto case BundleState.Loaded;
            // Do nothing
            case BundleState.Loaded: wasAlreadyLoaded = true; break;

            default: throw new ArgumentOutOfRangeException(nameof(bundle.State), bundle.State, null);
        }

        // If the bundle is already loaded, return it
        if (wasAlreadyLoaded)
        {
            result?.Set(bundle.LoadedBundle);
            if (bundle.LoadedBundle == null)
            {
                Plugin.Logger.LogError("Bundle was designated as loaded while it doesn't exist");
            }
            yield break;
        }

        // Load bundle
        bundle.State = BundleState.Loading;
        //      Patch bundle
        TaskResult<byte[]> patchRequest = new();
        yield return BZBundlePatcher.GetPatchedBundleAsync(bundlePath.location, patchRequest);
        //      Load patched bundle
        var request = AssetBundle.LoadFromMemoryAsync(patchRequest.value);
        yield return request;

        result?.Set(request.assetBundle);
        bundle.LoadedBundle = request.assetBundle;
        bundle.State = BundleState.Loaded;
    }

    internal static IEnumerator RemoveBundleUser(BundleFullPath path)
    {
        string id = GetIdentifierFromBundlePath(path);

        if (!_bundles.TryGetValue(id, out var bundle) || bundle.Users == 0)
        {
            Plugin.Logger.LogError("Critical error: releasing bundle with 0 users");
            yield break;
        }

        bundle.Users--;

        if (bundle.Users > 0)
        {
            // The bundle still has users, so, continue
            yield break;
        }

        bool shouldUnload;
        switch (bundle.State)
        {
            case BundleState.Unloaded:
                Plugin.Logger.LogWarning("Attempting to unload an already unloaded bundle");
                shouldUnload = false;
                break;
            case BundleState.Loading:
                Plugin.Logger.LogError("Attempting to unload bundle that is currently loading");
                yield return new WaitUntil(() => bundle.State == BundleState.Loaded);
                goto case BundleState.Loaded;
            case BundleState.Loaded: // Unload like normal
                shouldUnload = true;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(bundle.State), bundle.State, null);
        }

        if (!shouldUnload)
        {
            Plugin.Logger.LogWarning("Didn't unload bundle when reaching 0 users");
            yield break;
        }

        if (bundle.LoadedBundle == null)
        {
            Plugin.Logger.LogWarning("Cannot unload a bundle that is null");
            yield break;
        }

        bundle.LoadedBundle.Unload(true);
        bundle.State = BundleState.Unloaded;
        bundle.LoadedBundle = null;
    }

    private static string GetIdentifierFromBundlePath(BundleFullPath path) 
        => Path.GetFileName(path.location);

    private class BundleInstance
    {
        public AssetBundle LoadedBundle { get; set; }
        public int Users { get; set; }
        public BundleState State { get; set; }
    }

    private enum BundleState : byte
    {
        Unloaded,
        Loading,
        Loaded
    }
}