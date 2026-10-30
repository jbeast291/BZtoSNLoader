using UnityEngine;

namespace BelowZeroCreatures.MonoBehaviours;

public class PlayRandomTriggerAnimations : MonoBehaviour, IScheduledUpdateBehaviour
{
    public Animator animator;
    public string[] triggerNames;
    public float minInterval;
    public float maxInterval;

    private float _timePlayNext;
    
    public int scheduledUpdateIndex { get; set; }

    public string GetProfileTag()
    {
        return "BelowZeroCreatures.Fixes:PlayRandomTriggerAnimations";
    }

    public void ScheduledUpdate()
    {
        if (_timePlayNext > Time.time) return;
        
        _timePlayNext = Time.time + Random.Range(minInterval, maxInterval);
        
        animator.SetTrigger(triggerNames[Random.Range(0, triggerNames.Length)]);
    }

    private void OnEnable()
    {
        UpdateSchedulerUtils.Register(this);
    }

    private void OnDisable()
    {
        UpdateSchedulerUtils.Deregister(this);
    }
}