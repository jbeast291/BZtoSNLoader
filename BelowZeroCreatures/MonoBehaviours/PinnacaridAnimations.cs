using UnityEngine;

namespace BelowZeroCreatures.MonoBehaviours;

public class PinnacaridAnimations : MonoBehaviour, IManagedUpdateBehaviour
{
    private static readonly int OnSurface = Animator.StringToHash("on_surface");
    private static readonly int InWater = Animator.StringToHash("in_water");
    public Animator animator;
    public OnSurfaceTracker tracker;

    public int managedUpdateIndex { get; set; }

    public string GetProfileTag()
    {
        return "BelowZeroCreatures.Fixes:PinnacaridAnimations";
    }

    public void ManagedUpdate()
    {
        bool onSurface = tracker.onSurface;
        animator.SetBool(OnSurface, onSurface);
        animator.SetBool(InWater, !onSurface);
    }

    private void OnEnable()
    {
        BehaviourUpdateUtils.Register(this);
    }

    private void OnDisable()
    {
        BehaviourUpdateUtils.Deregister(this);
    }
}