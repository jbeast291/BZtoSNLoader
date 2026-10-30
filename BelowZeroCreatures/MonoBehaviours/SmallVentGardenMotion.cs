using UnityEngine;

namespace BelowZeroCreatures.MonoBehaviours;

public class SmallVentGardenMotion : MonoBehaviour
{
    public SwimBehaviour swimBehaviour;

    public Vector3 swimRadius = new Vector3(20f, 3f, 20f);

    public float swimForward = 0.5f;

    public float swimVelocity = 2f;

    public float swimInterval = 2f;
    
    private float _timeNextSwim;

    private void Update()
    {
        if (Time.time > _timeNextSwim)
        {
            Vector3 target = Random.onUnitSphere;
            target += transform.forward * swimForward;
            target = Vector3.Scale(target, swimRadius);
            Vector3 worldTarget = transform.position + target;
            _timeNextSwim = Time.time + swimInterval;
            swimBehaviour.SwimTo(worldTarget, swimVelocity);
        }
    }
}