using BelowZeroCreatures.CreatureModifiers.System;
using UnityEngine;

namespace BelowZeroCreatures.CreatureModifiers;

public class TriopsModifier : BzCreatureModifier
{
    public override string[] ClassIds => new[] { "Triops" };
    
    protected override string HoldAnimationParam => "eyeye";

    protected override void ModifyPrefab(GameObject prefab)
    {
        var fpModel = prefab.transform.Find("model_FP");
        fpModel.localPosition = new Vector3(0.050f, 0.000f, 0.030f);
        fpModel.localEulerAngles = new Vector3(53.428f, 77.143f, 351.429f);
        fpModel.localScale = Vector3.one * 0.6f;

        foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
        {
            var material = renderer.material;
            material.color = new Color(0.6f, 0.6f, 0.6f);
        }
    }
}