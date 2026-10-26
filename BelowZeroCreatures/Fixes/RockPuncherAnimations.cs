using UnityEngine;

namespace BelowZeroCreatures.Fixes;

public class RockPuncherAnimations : MonoBehaviour, IManagedUpdateBehaviour
{
    private static readonly int OnSurface = Animator.StringToHash("on_surface");
    public Animator animator;
    public OnSurfaceTracker tracker;

    public int managedUpdateIndex { get; set; }

    public string GetProfileTag()
    {
        return "BelowZeroCreatures.Fixes:RockPuncherAnimations";
    }

    public void ManagedUpdate()
    {
        animator.SetBool(OnSurface, tracker._onSurface);
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