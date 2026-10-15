using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BZtoSNLoader;

// This class is not safe to expose. You must request and release for users of a bundle.
internal class ManagedBundleLoader
{
    private readonly Dictionary<string, BundleInstance> _bundles = new(StringComparer.OrdinalIgnoreCase);

    private readonly IOut<AssetBundle> _disposedOut = new TaskResult<AssetBundle>();
    
    public IEnumerator RequestBundleAndAddUser(string path)
    {
        yield return RequestBundleAndAddUser(path, _disposedOut);
    }

    public IEnumerator RequestBundleAndAddUser(string path, IOut<AssetBundle> result)
    {
        string id = GetIdentifierFromBundlePath(path);
        
        if (!_bundles.TryGetValue(id, out var bundle))
        {
            bundle = new BundleInstance();
            _bundles.Add(id, bundle);
        }
        
        bundle.Users++;

        bool wasAlreadyLoaded;
        
        switch (bundle.State)
        {
            case BundleState.Unloaded:
                // Load the bundle
                wasAlreadyLoaded = false;
                break;
            case BundleState.Loading:
                // Wait for the bundle to finish loading
                yield return new WaitUntil(() => bundle.State == BundleState.Loaded);
                wasAlreadyLoaded = true;
                break;
            case BundleState.Loaded:
                // Do nothing
                wasAlreadyLoaded = true;
                break;
            case BundleState.Releasing:
                // Wait and restart (since we can't easily cancel the release)
                yield return new WaitUntil(() => bundle.State == BundleState.Releasing);
                wasAlreadyLoaded = false;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(bundle.State), bundle.State, null);
        }

        // If the bundle is already loaded, return it
        if (wasAlreadyLoaded)
        {
            result.Set(bundle.LoadedBundle);
            if (bundle.LoadedBundle == null)
            {
                Plugin.Logger.LogError("Bundle was designated as loaded while it doesn't exist");
            }
            yield break;
        }
        
        // Load bundle
        bundle.State = BundleState.Loading;
        var request = AssetBundle.LoadFromFileAsync(path);
        bundle.State = BundleState.Loaded;
        yield return request;
        result.Set(request.assetBundle);
        bundle.LoadedBundle = request.assetBundle;
    }
    
    public IEnumerator RemoveBundleUser(string path)
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
                shouldUnload = true;
                break;
            case BundleState.Loaded:
                // Unload like normal
                shouldUnload = true;
                break;
            case BundleState.Releasing:
                // Do nothing
                shouldUnload = false;
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
        
        bundle.State = BundleState.Releasing;
        bundle.LoadedBundle.Unload(true);
        bundle.State = BundleState.Unloaded;
        bundle.LoadedBundle = null;
    }
    
    private string GetIdentifierFromBundlePath(string fullBundlePath)
    {
        return Path.GetFileName(fullBundlePath);
    }

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
        Loaded,
        Releasing
    }
}