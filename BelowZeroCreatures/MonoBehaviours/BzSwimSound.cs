using BelowZeroCreatures.BzCreatureAudio;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BelowZeroCreatures.MonoBehaviours;

public class BzSwimSound : MonoBehaviour, IScheduledUpdateBehaviour
{
    [SerializeField] private FMODAsset[] sounds;
    [SerializeField] private float minDelay;
    [SerializeField] private float maxDelay;
    
    private float _timePlayNext;

    public static void ApplyToCreature(GameObject creaturePrefab, float delay, params string[] swimSounds)
    {
        var swim = creaturePrefab.AddComponent<BzSwimSound>();
        swim.sounds = new FMODAsset[swimSounds.Length];
        for (int i = 0; i < swimSounds.Length; i++)
        {
            swim.sounds[i] = BzCreaturesAudio.GetBzCreatureFmodAsset(swimSounds[i]);
        }

        swim.minDelay = delay;
        swim.maxDelay = delay + 0.5f;
    }

    private void Start()
    {
        _timePlayNext = Time.time + Random.Range(minDelay, maxDelay);
    }

    public void ScheduledUpdate()
    {
        if (Time.time < _timePlayNext)
        {
            return;
        }

        _timePlayNext = Time.time + Random.Range(minDelay, maxDelay);
        var sound = sounds[Random.Range(0, sounds.Length)];
        FMODUWE.PlayOneShot(sound, transform.position);
    }

    public string GetProfileTag()
    {
        return "BzSwimSound";
    }

    public int scheduledUpdateIndex { get; set; }

    private void OnEnable()
    {
        UpdateSchedulerUtils.Register(this);
    }

    private void OnDisable()
    {
        UpdateSchedulerUtils.Deregister(this);
    }
}