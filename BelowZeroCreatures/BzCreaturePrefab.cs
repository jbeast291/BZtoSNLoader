using System.Collections;
using System.Linq;
using BelowZeroCreatures.CreatureModifiers.System;
using BZtoSNLoader;
using ComponentPortFramework;
using Nautilus.Assets;
using Nautilus.Handlers;
using Nautilus.Utility;
using UnityEngine;

namespace BelowZeroCreatures;

public class BzCreaturePrefab
{
    public PrefabInfo Info { get; }
    private BZAssetLoader.AssetKey Key { get; }
    private JsonPrefab ComponentData { get; }
    
    private WaterParkCreatureData WaterParkData { get; set; }
    
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

        var techType = Info.TechType;
        BaseBioReactor.charge[techType] = 300;
        if (ModData.HoldableCreatures.Contains(Info.ClassID))
        {
            GravSphereHandler.AddAffectedTechType(techType);
        }
        
        CraftDataHandler.SetPickupSound(techType, "event:/loot/pickup_fish");

        if (ModData.CreatureData.TryGetValue(techType.ToString(), out var data))
            CreatureData.behaviourTypeList[techType] = data.BehaviourType;
        else
            Plugin.Logger.LogWarning("Did not find BehaviourType for creature: " + techType);

        RegisterWaterParkData();
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
        var cellLevel = GetCellLevel(prefab);
        
        // Essentials
        PrefabUtils.AddBasicComponents(prefab, Info.ClassID, Info.TechType, cellLevel);
        ApplyMaterials(prefab, Info.ClassID.EndsWith("_School"));
        
        // -- Various creature-related fixes --
        // Fix creature death
        var creatureDeath = prefab.GetComponent<CreatureDeath>();
        if (creatureDeath != null)
        {
            // The creature respawner WILL be a 'stale' reference and must be fixed to point to the right one
            TaskResult<GameObject> respawnerPrefabResult = new TaskResult<GameObject>();
            yield return CreatureDependencyUtils.GetCreatureRespawnerPrefab(respawnerPrefabResult);
            creatureDeath.respawnerPrefab = respawnerPrefabResult.Get();
        }

        // A valid sound is needed so we just remove this entirely
        foreach (var soundOnDamage in prefab.GetComponentsInChildren<SoundOnDamage>(true))
        {
            Object.DestroyImmediate(soundOnDamage);
        }

        // Apply creature-specific modifications
        if (CreatureModifiersManager.TryGetCreatureEdit(Info.ClassID, out var modifier))
        {
            modifier.ApplyModifications(prefab);
        }
        
        // Live mixin
        LiveMixin lm = prefab.GetComponent<LiveMixin>();
        if (lm != null)
        {
            var lmData = ScriptableObject.CreateInstance<LiveMixinData>();
            if (ModData.CreatureData.TryGetValue(Info.ClassID, out var creatureData))
            {
                lmData.maxHealth = creatureData.MaxHealth;
            }
            else
            {
                lmData.maxHealth = 100;
                Plugin.Logger.LogWarning("Couldn't find max health for creature: " + Info.ClassID);
            }
            lm.data = lmData;
            lm.health = lmData.maxHealth;
            lm.defaultHealth = lmData.maxHealth;
            TaskResult<GameObject> damageEffectResult = new TaskResult<GameObject>();
            yield return CreatureDependencyUtils.GetBloodParticlePrefab(damageEffectResult);
            lm.data.damageEffect = damageEffectResult.Get();
        }
        
        // Infectability
        if (ModData.CreatureData.TryGetValue(Info.ClassID, out var data) &&
            data.BehaviourType != BehaviourType.Leviathan && Info.ClassID != "GlowWhale" && !Info.ClassID.EndsWith("School"))
        {
            var infectableRenderers = prefab.GetComponentsInChildren<Renderer>(true).Where(r =>
                r.sharedMaterials.All(m => m.shader != null && m.shader.name.Equals("MarmosetUBER"))).ToArray();
            if (infectableRenderers.Length > 0)
            {
                var infectedMixin = prefab.AddComponent<InfectedMixin>();
                infectedMixin.renderers = infectableRenderers;
            }
        }

        if (prefab.TryGetComponent<WaterParkCreature>(out var waterParkCreature))
        {
            waterParkCreature.data = WaterParkData;
        }
        
        result.Set(prefab);
    }

    private LargeWorldEntity.CellLevel GetCellLevel(GameObject prefab)
    {
        LargeWorldEntity.CellLevel cellLevel = LargeWorldEntity.CellLevel.Far;
        
        if (prefab.GetComponent<Pickupable>() != null)
            cellLevel = LargeWorldEntity.CellLevel.Medium;
        
        if (prefab.TryGetComponent<LargeWorldEntity>(out var largeWorldEntity))
            cellLevel = largeWorldEntity.cellLevel;
        else
            Plugin.Logger.LogWarning($"Couldn't find correct CellLevel for {prefab}");

        if (Info.ClassID == "Pinnacarid" || Info.ClassID == "RockPuncher")
            cellLevel = LargeWorldEntity.CellLevel.Near; // so they don't fall through terrain
        
        return cellLevel;
    }   

    private void ApplyMaterials(GameObject prefab, bool isSchool)
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
                if (!isSchool)
                    m.SetFloat("_SpecInt", m.GetFloat("_SpecInt") * 2);
                m.SetFloat("_InfectionHeightStrength", 0);
            }
            renderer.materials = materials;
        }
    }

    private void RegisterWaterParkData()
    {
        WaterParkData = ScriptableObject.CreateInstance<WaterParkCreatureData>();
        WaterParkData.daysToGrow = 2;
        WaterParkData.eggOrChildPrefab = new CustomGameObjectReference(Info.ClassID);
        bool isChild = false;
        if (Info.ClassID == "SeaMonkeyBaby")
        {
            WaterParkData.adultPrefab = new CustomGameObjectReference("SeaMonkey");
            isChild = true;
            WaterParkData.initialSize = 1;
        }
        else if (Info.ClassID == "PenguinBaby")
        {
            WaterParkData.adultPrefab = new CustomGameObjectReference("Penguin");
            isChild = true;
            WaterParkData.initialSize = 0.5f;
        }
        else if (Info.ClassID == "SeaMonkey")
        {
            WaterParkData.eggOrChildPrefab = new CustomGameObjectReference("SeaMonkeyBaby");
        }
        else if (Info.ClassID == "Penguin")
        {
            WaterParkData.eggOrChildPrefab = new CustomGameObjectReference("PenguinBaby");
        }

        if (!isChild)
        {
            WaterParkData.initialSize = 0.35f;
        }
        WaterParkData.canBreed = !isChild;
        WaterParkData.isPickupableOutside = ModData.HoldableCreatures.Contains(Info.ClassID);
        WaterParkData.outsideSize = 1;
    }
}