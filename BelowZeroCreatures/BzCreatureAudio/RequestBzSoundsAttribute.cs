using System;

namespace BelowZeroCreatures.BzCreatureAudio;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class RequestBzSoundsAttribute : Attribute
{
    public RequestBzSoundsAttribute(float maxDistance, params string[] soundNames)
    {
        MaxDistance = maxDistance;
        SoundNames = soundNames;
    }

    internal float MaxDistance { get; private set; }
    internal string[] SoundNames { get; private set; }
}