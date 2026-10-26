using System.Collections;
using BelowZeroCreatures.Fixes;
using BZtoSNLoader;
using ComponentPortFramework;
using Nautilus.Assets;
using Nautilus.Utility;
using UnityEngine;

namespace BelowZeroCreatures;

public class BzCreaturePrefab
{
    public PrefabInfo Info { get; }
    private BZAssetLoader.AssetKey Key { get; }
    private JsonPrefab ComponentData { get; }
    
    public BzCreaturePrefab(PrefabInfo info, BZAssetLoader.AssetKey assetKey, JsonPrefab componentData)
    {
        Info = info;
        Key = assetKey;
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
        // Load assets
        TaskResult<GameObject> initialLoadResult = new TaskResult<GameObject>();
        yield return BZAssetLoader.LoadAssetAsync(Key, initialLoadResult);
        var originalPrefab = initialLoadResult.Get();
        var prefab = UWE.Utils.InstantiateDeactivated(originalPrefab);
        
        // Reconstruct components
        Plugin.ComponentSerializer.ReconstructComponentsOnGameObject(prefab, ComponentData);
        
        // Cell level
        LargeWorldEntity.CellLevel cellLevel = LargeWorldEntity.CellLevel.Far;
        
        if (prefab.GetComponent<Pickupable>() != null)
            cellLevel = LargeWorldEntity.CellLevel.Medium;
        
        if (prefab.TryGetComponent<LargeWorldEntity>(out var largeWorldEntity))
            cellLevel = largeWorldEntity.cellLevel;
        else
            Plugin.Logger.LogWarning($"Couldn't find correct CellLevel for {prefab}");
        
        // Essentials
        PrefabUtils.AddBasicComponents(prefab, Info.ClassID, Info.TechType, cellLevel);
        ApplyMaterials(prefab);
        
        // -- Various creature-related fixes --
        // Fix creature death
        var creatureDeath = prefab.GetComponent<CreatureDeath>();
        if (creatureDeath != null)
        {
            // The creature respawner WILL be a 'stale' reference and must be fixed to point to the right one
            TaskResult<GameObject> respawnerPrefabResult = new TaskResult<GameObject>();
            yield return CreatureRespawnerUtils.GetCreatureRespawnerPrefab(respawnerPrefabResult);
            creatureDeath.respawnerPrefab = respawnerPrefabResult.Get();
        }

        // A valid sound is needed so we just remove this entirely
        foreach (var soundOnDamage in creatureDeath.GetComponentsInChildren<SoundOnDamage>())
        {
            Object.DestroyImmediate(soundOnDamage);
        }
        
        HardcodedCreatureFixes.ApplyFixesByCreature(prefab, Info.ClassID);
        
        result.Set(prefab);
    }

    private static void ApplyMaterials(GameObject prefab)
    {
        foreach (var renderer in prefab.GetComponentsInChildren<Renderer>())
        {
            var materials = renderer.materials;
            foreach (var m in materials)
            {
                if (m == null) continue;
                if (!m.shader.name.Contains("MarmosetUBER")) continue;
                if (m.GetInt("_ZWrite") == 0) continue; // Skip transparent materials
                m.shader = MaterialUtils.Shaders.MarmosetUBER;
            }
            renderer.materials = materials;
        }
    }
    
}