using ComponentPortFramework;
using Nautilus.Utility;
using UnityEngine;

namespace BelowZeroCreatures.Fixes;

internal static class HardcodedCreatureFixes
{
    internal static void ApplyFixesByCreature(GameObject prefab, string classId)
    {
        switch (classId)
        {
            case "Penguin":
                ApplyPenguinFixes(prefab);
                break;
            case "Pinnacarid":
                ApplyPinnacaridFixes(prefab);
                break;
            case "RockPuncher":
                ApplyRockPuncherFixes(prefab);
                break;
            case "SquidShark":
                ApplySquidsharkFixes(prefab);
                break;
            case "SquidShark_LilyPads":
                ApplySquidsharkFixes(prefab);
                break;
            case "SmallVentGarden":
                ApplyVentGardenJuvenileFixes(prefab);
                break;
            case "TitanHolefish":
                ApplyTitanHolefishFixes(prefab);
                break;
        }
    }

    private static void ApplyPenguinFixes(GameObject prefab)
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
    
    private static void ApplyPinnacaridFixes(GameObject prefab)
    {
        var animator = prefab.GetComponentInChildren<Animator>();
        
        var animations = prefab.AddComponent<PinnacaridAnimations>();
        animations.animator = animator;
        animations.tracker = prefab.GetComponent<OnSurfaceTracker>();
        prefab.AddComponent<FreezeRigidbodyWhenFar>().freezeDist = 43;
    }
    
    private static void ApplyRockPuncherFixes(GameObject prefab)
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
    
    private static void ApplySquidsharkFixes(GameObject prefab)
    {
        // attack last target
        var component = prefab.AddComponent<AttackLastTarget>();
        component.evaluatePriority = 0.6f;
        component.swimVelocity = 8;
        component.aggressionThreshold = 0.75f;
        component.minAttackDuration = 3;
        component.maxAttackDuration = 7;
        component.pauseInterval = 7;
        component.rememberTargetTime = 7;
        component.resetAggressionOnTime = true;
        component.lastTarget = prefab.GetComponent<LastTarget>();
        component.minActionCheckInterval = 3;

        // Later, we could make this do proper electrical damage!
        foreach (var onTouchFixer in prefab.GetComponentsInChildren<OnTouchFixer>())
        {
            if (onTouchFixer.gameObject.name == "mouth")
            {
                continue;
            }
            
            // replace or remove electrical damage trigger fixer
            Object.DestroyImmediate(onTouchFixer);
        }
    }

    private static void ApplyVentGardenJuvenileFixes(GameObject prefab)
    {
        var motion = prefab.AddComponent<SmallVentGardenMotion>();
        motion.swimBehaviour = prefab.GetComponent<SwimBehaviour>();
    }
    
    private static void ApplyTitanHolefishFixes(GameObject prefab)
    {
        // I think these avoid their own BODIES, so I change this value
        var avoid = prefab.GetComponent<AvoidObstacles>();
        avoid.avoidTerrainOnly = true;
        avoid.avoidanceDistance = 12;
        avoid.scanDistance = 12;
        // add oxygen provider
        var air = prefab.transform.Find("Collisions/AirSack");
        var target = air.gameObject.AddComponent<TitanHolefishOxygenHandTarget>();
        target.liveMixin = prefab.GetComponent<LiveMixin>();
        target.sound = AudioUtils.GetFmodAsset("event:/env/bubble_pop_player");
    }
}