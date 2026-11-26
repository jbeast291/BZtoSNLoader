using BelowZeroCreatures.BzCreatureAudio;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BelowZeroCreatures.MonoBehaviours;

public class BzCreatureVoice : MonoBehaviour, IScheduledUpdateBehaviour
{
    private static readonly int CallVariantParam = Animator.StringToHash("callVariant");
    [SerializeField] private int roarTriggerParam;
    [SerializeField] internal FMOD_CustomEmitter emitter;
    [SerializeField] private Animator animator;
    [SerializeField] private FMODAsset[] sounds;
    [SerializeField] private float minDelay;
    [SerializeField] private float maxDelay;
    [SerializeField] internal int minCallVariant = 1;
    [SerializeField] internal int maxCallVariant;
    [SerializeField] internal bool callVariantMatchesSoundIndex;
    [SerializeField] internal Transform emitterTransform;
    
    private float _timePlayNext;

    public static BzCreatureVoice StartApplyingToCreature(GameObject creaturePrefab, string triggerName, float minDelay, float maxDelay,
        params string[] roarSounds)
    {
        var roar = creaturePrefab.AddComponent<BzCreatureVoice>();
        roar.animator = creaturePrefab.GetComponentInChildren<Animator>();
        roar.sounds = new FMODAsset[roarSounds.Length];
        for (int i = 0; i < roarSounds.Length; i++)
        {
            roar.sounds[i] = BzCreaturesAudio.GetBzCreatureFmodAsset(roarSounds[i]);
        }

        roar.minDelay = minDelay;
        roar.maxDelay = maxDelay;
        
        roar.roarTriggerParam = Animator.StringToHash(triggerName);
        
        roar.emitterTransform = creaturePrefab.transform;

        return roar;
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
        int soundIndex = Random.Range(0, sounds.Length);
        if (callVariantMatchesSoundIndex)
        {
            animator.SetInteger(CallVariantParam, soundIndex + minCallVariant);
        }
        else if (maxCallVariant > 0)
        {
            animator.SetInteger(CallVariantParam, Random.Range(minCallVariant, maxCallVariant + 1));
        }
        if (animator) animator.SetTrigger(roarTriggerParam);
        var roarSound = sounds[soundIndex];
        emitter.SetAsset(roarSound);
        emitter.Play();
    }

    public string GetProfileTag()
    {
        return "BzCreatureRoar";
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

public static class BzCreatureVoiceExtensions
{
    public static BzCreatureVoice WithCallAnimationVariants(this BzCreatureVoice voice, int maxCallVariant, int minCallVariant = 1)
    {
        voice.minCallVariant = minCallVariant;
        voice.maxCallVariant = maxCallVariant;
        return voice;
    }

    public static BzCreatureVoice WithCallAnimationMatchingSoundIndex(this BzCreatureVoice voice)
    {
        voice.callVariantMatchesSoundIndex = true;
        voice.minCallVariant = 1;
        return voice;
    }
    
    public static BzCreatureVoice WithChildEmitterTransform(this BzCreatureVoice voice, Transform emitterTransform)
    {
        voice.emitterTransform = emitterTransform;
        return voice;
    }

    public static void CompleteAndApply(this BzCreatureVoice voice)
    {
        // Add emitter
        var emitter = voice.emitterTransform.gameObject.AddComponent<FMOD_CustomEmitter>();
        emitter.followParent = true;
        emitter.playOnAwake = false;
        voice.emitter = emitter;
    }
}