using System.Collections;
using BZtoSNLoader;
using ComponentPortFramework;
using Nautilus.Assets;
using Nautilus.Utility;
using UnityEngine;

namespace BelowZeroPrefabs;

public class BzPrefab
{
    public PrefabInfo Info { get; }
    private BelowZeroAssetLoader.AddressablesAssetPath AddressablePath { get; }
    private JsonPrefab ComponentData { get; }

    public BzPrefab(PrefabInfo info, BelowZeroAssetLoader.AddressablesAssetPath assetPath, JsonPrefab componentData)
    {
        Info = info;
        AddressablePath = assetPath;
        ComponentData = componentData;
    }

    public void Register()
    {
        CustomPrefab nautilusPrefab = new CustomPrefab(Info);
        nautilusPrefab.SetGameObject(GetPrefabAsync);
        nautilusPrefab.Register();
    }

    private IEnumerator GetPrefabAsync(IOut<GameObject> result)
    {
        TaskResult<GameObject> initialLoadResult = new TaskResult<GameObject>();
        yield return BZtoSNLoader.Plugin.AssetLoader.LoadAssetAsync(AddressablePath, initialLoadResult);
        var originalPrefab = initialLoadResult.Get();
        var prefab = UWE.Utils.InstantiateDeactivated(originalPrefab);
        Plugin.ComponentSerializer.ReconstructComponentsOnGameObject(prefab, ComponentData);
        LargeWorldEntity.CellLevel cellLevel = LargeWorldEntity.CellLevel.Far;
        if (prefab.TryGetComponent<LargeWorldEntity>(out var largeWorldEntity))
            cellLevel = largeWorldEntity.cellLevel;
        PrefabUtils.AddBasicComponents(prefab, Info.ClassID, Info.TechType, cellLevel);
        result.Set(prefab);
    }
}