using BelowZeroCreatures.CreatureModifiers.System;
using UnityEngine;

namespace BelowZeroCreatures.CreatureModifiers;

public class TriopsModifier : BzCreatureModifier
{
    public override string[] ClassIds => new[] { "Triops" };
    
    protected override string HoldAnimationParam => "eyeye";

    protected override void ModifyPrefab(GameObject prefab) { }
}