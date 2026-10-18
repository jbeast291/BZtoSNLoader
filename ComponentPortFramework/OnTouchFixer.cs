using UnityEngine;

namespace ComponentPortFramework;

public class OnTouchFixer : MonoBehaviour
{
    public OnTouch onTouch;
    public MeleeAttack melee;

    private void OnEnable()
    {
        onTouch.onTouch.AddListener(melee.OnTouch);
    }
    
    private void OnDisable()
    {
        onTouch.onTouch.RemoveListener(melee.OnTouch);
    }
}