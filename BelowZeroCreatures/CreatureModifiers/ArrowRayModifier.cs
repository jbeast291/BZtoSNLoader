using BelowZeroCreatures.CreatureModifiers.System;
using UnityEngine;

namespace BelowZeroCreatures.CreatureModifiers;

public class ArrowRayModifier : BzCreatureModifier
{
    public override string[] ClassIds => new[] { "ArrowRay" };
    protected override string HoldAnimationParam => "boomerang";

    protected override void ModifyPrefab(GameObject prefab)
    {
        var fpModel = prefab.transform.Find("model_FP");
        fpModel.localPosition = new Vector3(0, -0.01f, -0.02f);
        fpModel.localEulerAngles = new Vector3(300, 334.2f, 180);
    }
}