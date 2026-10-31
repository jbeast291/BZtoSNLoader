using System.Collections;
using BZtoSNLoader;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using Nautilus.Handlers;
using Nautilus.Utility;
using UnityEngine;

namespace BelowZeroCreatures;

public class CookedCreature
{
    public PrefabInfo CreatureInfo { get; }
    public PrefabInfo Info { get; }
    
    private bool IsCured { get; }
    private string AssetName { get; }

    public CookedCreature(PrefabInfo creatureInfo, bool cured)
    {
        CreatureInfo = creatureInfo;
        IsCured = cured;
        AssetName = (cured ? "Cured" : "Cooked") + ModData.FixCookedCreatureAssetPath(creatureInfo.ClassID);
        string classId = (cured ? "Cured" : "Cooked") + ModData.FixCookedCreatureTechTypeName(creatureInfo.ClassID);
        Info = PrefabInfo.WithTechType(classId)
            .WithFolderPath("Below Zero Creatures/Food");
    }

    public void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(GetPrefab);
        prefab.SetRecipe(GetRecipe())
            .WithFabricatorType(CraftTree.Type.Fabricator)
            .WithStepsToFabricatorTab(IsCured
                ? CraftTreeHandler.Paths.FabricatorCuredFood
                : CraftTreeHandler.Paths.FabricatorCookedFood);
        prefab.SetPdaGroupCategory(TechGroup.Survival, IsCured ? TechCategory.CuredFood : TechCategory.CookedFood);
        prefab.Register();
    }

    private RecipeData GetRecipe()
    {
        if (IsCured)
        {
            return new RecipeData(new Ingredient(CreatureInfo.TechType, 1),
                new Ingredient(TechType.Salt, 1));
        }

        return new RecipeData(new Ingredient(CreatureInfo.TechType, 1));
    }

    private IEnumerator GetPrefab(IOut<GameObject> result)
    {
        string assetPath = $"WorldEntities/Food/{AssetName}.prefab";
        TaskResult<GameObject> initialLoadResult = new TaskResult<GameObject>();
        yield return BZAssetLoader.LoadAssetAsync(new BZAssetLoader.AssetKey(assetPath), initialLoadResult);
        var originalPrefab = initialLoadResult.Get();
        var prefab = UWE.Utils.InstantiateDeactivated(originalPrefab);
        PrefabUtils.AddBasicComponents(prefab, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Near);
        prefab.AddComponent<Pickupable>();
        result.Set(prefab);
    }
}