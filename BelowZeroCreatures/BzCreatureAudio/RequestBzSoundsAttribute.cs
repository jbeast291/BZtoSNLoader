using System;
using Nautilus.Utility;

namespace BelowZeroCreatures.BzCreatureAudio;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class RequestBzSoundsAttribute : Attribute
{
    public RequestBzSoundsAttribute(float maxDistance, params string[] soundNames)
    {
        SoundRequests = new SoundRequest[soundNames.Length];
        for (int i = 0; i < soundNames.Length; i++)
        {
            SoundRequests[i] = new SoundRequest()
            {
                EventName = soundNames[i],
                MinDistance = 2,
                MaxDistance = maxDistance,
                BzSounds = new string[] { soundNames[i] }
            };
        }
    }

    public RequestBzSoundsAttribute(string multiEventName, float maxDistance, params string[] soundNames)
    {
        SoundRequests = new SoundRequest[]
        {
            new()
            {
                EventName = multiEventName,
                MinDistance = 5,
                MaxDistance = maxDistance,
                BzSounds = soundNames
            }
        };
    }

    internal SoundRequest[] SoundRequests { get; }
    internal float MaxDistance { get; private set; }
    internal string MultiEventName { get; private set; }
    internal string[] SoundNames { get; private set; }
    public string Bus { internal get; init; } = AudioUtils.BusPaths.UnderwaterCreatures;
}