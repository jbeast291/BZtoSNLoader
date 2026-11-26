using System;

namespace BelowZeroCreatures.BzCreatureAudio;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class RequestBzSoundsAttribute : Attribute
{
    public RequestBzSoundsAttribute(float minDistance, float maxDistance, params string[] soundNames)
    {
        SoundRequests = new SoundRequest[soundNames.Length];
        for (int i = 0; i < soundNames.Length; i++)
        {
            SoundRequests[i] = new SoundRequest()
            {
                EventName = soundNames[i],
                MinDistance = minDistance,
                MaxDistance = maxDistance,
                BzSounds = new string[] { soundNames[i] }
            };
        }
    }

    public RequestBzSoundsAttribute(string multiEventName, float minDistance, float maxDistance, params string[] soundNames)
    {
        SoundRequests = new SoundRequest[]
        {
            new()
            {
                EventName = multiEventName,
                MinDistance = minDistance,
                MaxDistance = maxDistance,
                BzSounds = soundNames
            }
        };
    }

    internal SoundRequest[] SoundRequests { get; }
}