using BelowZeroCreatures.CreatureModifiers.System;
using BelowZeroCreatures.MonoBehaviours;
using UnityEngine;

namespace BelowZeroCreatures.CreatureModifiers;

public class RockPuncherModifier : BzCreatureModifier
{
    public override string[] ClassIds => new[] { "RockPuncher" };

    protected override void ModifyPrefab(GameObject prefab)
    {
        var animator = prefab.GetComponentInChildren<Animator>();

        var animate = prefab.AddComponent<AnimateByVelocity>();
        animate.levelOfDetail = prefab.GetComponent<BehaviourLOD>();
        animate.rootGameObject = prefab;
        animate.animator = animator;
        animate.animationMoveMaxSpeed = 1.5f;

        var animations = prefab.AddComponent<RockPuncherAnimations>();
        animations.animator = animator;
        animations.tracker = prefab.GetComponent<OnSurfaceTracker>();
    }
}