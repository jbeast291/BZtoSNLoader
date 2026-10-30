using BelowZeroCreatures.CreatureModifiers.System;
using BelowZeroCreatures.MonoBehaviours;
using UnityEngine;

namespace BelowZeroCreatures.CreatureModifiers;

public class PenguinModifier : BzCreatureModifier
{
    public override string[] ClassIds => new[] { "Penguin" };

    protected override void ModifyPrefab(GameObject prefab)
    {
        var animator = prefab.GetComponentInChildren<Animator>();
        prefab.AddComponent<SetAnimationOverrideOnStart>().SetUpBoolValue(animator, "on_surface", false);
        prefab.AddComponent<SetAnimationOverrideOnStart>().SetUpBoolValue(animator, "in_water", true);

        prefab.GetComponentInChildren<AnimateByVelocity>().animationMoveMaxSpeed = 2.5f;

        var randomTriggerAnims = prefab.AddComponent<PlayRandomTriggerAnimations>();
        randomTriggerAnims.animator = animator;
        randomTriggerAnims.minInterval = 3;
        randomTriggerAnims.maxInterval = 9;
        randomTriggerAnims.triggerNames = new[]
        {
            "seal_check",
            "swim_idle",
            "barrell_roll"
        };

        var collisions = prefab.transform.Find("collisions").gameObject;
        var spheres = collisions.GetComponents<SphereCollider>();
        spheres[0].enabled = false;
        spheres[1].enabled = true;
    }
}