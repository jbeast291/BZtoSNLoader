using BelowZeroCreatures.CreatureModifiers.System;
using BelowZeroCreatures.MonoBehaviours;
using UnityEngine;

namespace BelowZeroCreatures.CreatureModifiers;

public class PinnacaridModifier : BzCreatureModifier
{
    public override string[] ClassIds => new[] { "Pinnacarid" };

    protected override void ModifyPrefab(GameObject prefab)
    {
        var animator = prefab.GetComponentInChildren<Animator>();
        
        var animations = prefab.AddComponent<PinnacaridAnimations>();
        animations.animator = animator;
        animations.tracker = prefab.GetComponent<OnSurfaceTracker>();
        prefab.AddComponent<FreezeRigidbodyWhenFar>().freezeDist = 43;
    }
}