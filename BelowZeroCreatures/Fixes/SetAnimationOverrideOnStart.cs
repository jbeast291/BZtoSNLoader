using UnityEngine;

namespace BelowZeroCreatures.Fixes;

public class SetAnimationOverrideOnStart : MonoBehaviour
{
    public Animator animator;
    
    [SerializeField] private string parameterName;
    [SerializeField] private float floatValue;
    [SerializeField] private bool boolValue;
    [SerializeField] private Mode mode;

    private void Start()
    {
        if (mode == Mode.Float)
        {
            animator.SetFloat(parameterName, floatValue);
        }
        else if (mode == Mode.Bool)
        {
            animator.SetBool(parameterName, boolValue);
        }
    }

    private enum Mode
    {
        Float,
        Bool
    }

    public void SetUpFloatValue(Animator animatorController, string paramName, float val)
    {
        animator = animatorController;
        mode = Mode.Float;
        parameterName = paramName;
        floatValue = val;
    }
    
    public void SetUpBoolValue(Animator animatorController, string paramName, bool val)
    {
        animator = animatorController;
        mode = Mode.Bool;
        parameterName = paramName;
        boolValue = val;
    }
}