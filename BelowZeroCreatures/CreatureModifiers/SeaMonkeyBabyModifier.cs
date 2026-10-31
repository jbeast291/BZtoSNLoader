using BelowZeroCreatures.CreatureModifiers.System;
using UnityEngine;

namespace BelowZeroCreatures.CreatureModifiers;

public class SeaMonkeyBabyModifier : BzCreatureModifier
{
    public override string[] ClassIds => new[] { "SeaMonkeyBaby" };
    
    protected override string HoldAnimationParam => "beacon";

    protected override void ModifyPrefab(GameObject prefab)
    {
        var fpModel = prefab.transform.Find("modelFP");
        fpModel.localPosition = new Vector3(0, 0, 0.12f);
        fpModel.localEulerAngles = new Vector3(15, 0, 0);
    }
}