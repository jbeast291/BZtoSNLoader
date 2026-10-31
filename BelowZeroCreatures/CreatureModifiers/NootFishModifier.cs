using BelowZeroCreatures.CreatureModifiers.System;
using UnityEngine;

namespace BelowZeroCreatures.CreatureModifiers;

public class NootFishModifier : BzCreatureModifier
{
    public override string[] ClassIds => new[] { "NootFish" };
    protected override string HoldAnimationParam => "peeper";

    protected override void ModifyPrefab(GameObject prefab)
    {
        var fpModel = prefab.transform.Find("model_FP");
        fpModel.localPosition = new Vector3(-0.020f, 0, 0);
        fpModel.localEulerAngles = new Vector3(351, 10, 0);
        foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
        {
            var material = renderer.material;
            material.color = Color.white;
            material.SetColor("_SpecColor", Color.white);
            material.SetFloat("_Shininess", 7);
        }
    }
}