using BelowZeroCreatures.CreatureModifiers.System;
using UnityEngine;

namespace BelowZeroCreatures.CreatureModifiers;

public class PenguinBabyModifier : BzCreatureModifier
{
    public override string[] ClassIds => new[] { "PenguinBaby" };
    protected override string HoldAnimationParam => "beacon";

    protected override void ModifyPrefab(GameObject prefab)
    {
        var fpModel = prefab.transform.Find("model_fp");
        fpModel.localPosition = new Vector3(0, -0.118f, -0.1f);
        fpModel.localEulerAngles = new Vector3(17, 0, 0);
    }
}