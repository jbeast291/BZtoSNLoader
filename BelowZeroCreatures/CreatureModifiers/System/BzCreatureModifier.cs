using BelowZeroCreatures.BzCreatureAudio;
using ComponentPortFramework;
using UnityEngine;

namespace BelowZeroCreatures.CreatureModifiers.System;

public abstract class BzCreatureModifier
{
    public abstract string[] ClassIds { get; }
    protected abstract void ModifyPrefab(GameObject prefab);
    protected virtual string HoldAnimationParam => "peeper";

    public void ApplyModifications(GameObject prefab)
    {
        var dropTool = prefab.GetComponent<LazyDropToolFix>();
        if (dropTool != null)
        {
            dropTool.customAnimToolName = HoldAnimationParam;
        }
        ModifyPrefab(prefab);
    }

    protected void AddMeleeAttackSound(GameObject prefab, FMODAsset asset, float interval, Transform bone = null)
    {
        Transform toAddTo = prefab.transform;
        if (bone) toAddTo = bone;
        var biteEmitter = toAddTo.gameObject.AddComponent<FMOD_StudioEventEmitter>();
        biteEmitter.path = asset.path;
        biteEmitter.minInterval = interval;
        prefab.GetComponent<MeleeAttack>().attackSound = biteEmitter;
    }
}