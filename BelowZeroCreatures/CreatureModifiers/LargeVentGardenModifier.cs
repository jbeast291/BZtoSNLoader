using BelowZeroCreatures.CreatureModifiers.System;
using BelowZeroCreatures.MonoBehaviours;
using UnityEngine;

namespace BelowZeroCreatures.CreatureModifiers;

public class LargeVentGardenModifier : BzCreatureModifier
{
    public override string[] ClassIds => new[] { "LargeVentGarden" };
    protected override void ModifyPrefab(GameObject prefab)
    {
        var animations = prefab.AddComponent<LargeVentGardenAnimations>();
        animations.animator = prefab.GetComponentInChildren<Animator>();
    }
}