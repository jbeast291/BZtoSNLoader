using System;
using System.Collections.Generic;

namespace BelowZeroCreatures.BzCreatureAudio;

internal static class BzSoundRequestLocator
{
    public static IEnumerable<SoundRequest> GetAllRequestedBelowZeroSounds()
    {
        var set = new List<SoundRequest>();
        foreach (Type type in Plugin.Assembly.GetTypes())
        {
            var attributes = type.GetCustomAttributes(typeof(RequestBzSoundsAttribute), false);
            foreach (var attribute in attributes)
            {
                if (attribute is not RequestBzSoundsAttribute soundRequest) continue;
                
                set.AddRange(soundRequest.SoundRequests);
            }
        }
        return set;
    }
}