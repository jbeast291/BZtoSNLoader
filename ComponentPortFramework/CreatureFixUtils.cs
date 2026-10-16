using UnityEngine;

namespace ComponentPortFramework;

public static class CreatureFixUtils
{
    public static void FixCreatureComponentIssues(Component component)
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
    }
}