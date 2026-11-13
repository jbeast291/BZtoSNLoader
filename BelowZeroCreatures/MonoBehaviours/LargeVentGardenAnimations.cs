using UnityEngine;

namespace BelowZeroCreatures.MonoBehaviours;

public class LargeVentGardenAnimations : MonoBehaviour
{
    public Animator animator;

    private void Start()
    {
        animator.SetBool("inhale", true);
    }

    private void OnEnable()
    {
        animator.SetBool("inhale", true);
    }
}