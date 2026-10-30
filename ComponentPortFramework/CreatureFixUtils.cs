using BepInEx.Logging;
using UnityEngine;

namespace ComponentPortFramework;

public static class CreatureFixUtils
{
    public static void FixCreatureComponentIssues(Component component, ILogger logger)
    {
        // TODO: Use proper health values
        if (component is LiveMixin lm)
        {
            var lmData = ScriptableObject.CreateInstance<LiveMixinData>(); 
            lm.data = lmData;
            lm.health = lmData.maxHealth;
            lm.defaultHealth = lmData.maxHealth;
        }

        if (component is Creature creature)
        {
            var liveMixin = creature.GetComponent<LiveMixin>();
            creature.liveMixin = liveMixin;
            // DOES THIS NEED TO BE DONE? WHY??
            creature.traitsAnimator = null;
            creature.traitsAnimator = creature.GetComponentInChildren<Animator>();
            foreach (var dead in creature.GetComponentsInChildren<DeadAnimationOnEnable>(true))
            {
                dead.liveMixin = liveMixin;
            }
        }
            
        if (component is AnimateByVelocity abv)
        {
            // WHY DOES THIS NEED TO BE DONE? DOES IT EVEN NEED THIS!????
            abv.animator = abv.GetComponentInChildren<Animator>();
        }

        if (component is WorldForces wf)
        {
            wf.handleGravity = true;
        }

        if (component is MeleeAttack melee)
        {
            var onTouches = melee.gameObject.GetComponentsInChildren<OnTouch>(true);
            if (onTouches != null)
            {
                foreach (var onTouch in onTouches)
                {
                    if (onTouch.gameObject.TryGetComponent<OnTouchFixer>(out var onTouchFixer))
                    {
                        logger.Log(LogLevel.Warning, "Trying to add multiple MeleeAttack callbacks to a single OnTouch component");
                    }
                    else
                    {
                        onTouchFixer = onTouch.gameObject.AddComponent<OnTouchFixer>();
                    }

                    onTouchFixer.onTouch = onTouch;
                    onTouchFixer.melee = melee;
                }
            }
        }

        if (component is LazyDropToolFix dropTool)
        {
            dropTool.drawSound = null;
            dropTool.drawSoundUnderwater = null;
            dropTool.hitBleederSound = null;
            dropTool.holsterSoundAboveWater = null;
            dropTool.holsterSoundUnderwater = null;
            dropTool.reloadSound = null;
        }
    }
}