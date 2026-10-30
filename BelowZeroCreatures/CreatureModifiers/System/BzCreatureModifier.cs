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
    }
}