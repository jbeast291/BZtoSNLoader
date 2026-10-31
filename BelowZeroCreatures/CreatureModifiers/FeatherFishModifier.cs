using BelowZeroCreatures.CreatureModifiers.System;
using UnityEngine;

namespace BelowZeroCreatures.CreatureModifiers;

public class FeatherFishModifier : BzCreatureModifier
{
    public override string[] ClassIds => new[] { "FeatherFish" };
    protected override string HoldAnimationParam => "boomerang";

    protected override void ModifyPrefab(GameObject prefab)
    {
        var fpModel = prefab.transform.Find("model_FP");
        fpModel.localPosition = new Vector3(0.03f, -0.03f, -0.03f);
        fpModel.localEulerAngles = new Vector3(316.1f, 18.14f, 0);
        foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
        {
            var material = renderer.material;
            material.color = Color.white;
            material.SetFloat("_SpecInt", 2);
            material.SetFloat("_Shininess", 7);
            material.SetFloat("_Fresnel", 0.3f);
        }
    }
}