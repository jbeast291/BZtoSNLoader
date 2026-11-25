using System;
using System.Collections.Generic;

namespace BelowZeroCreatures.BzCreatureAudio;

internal static class BzSoundRequestLocator
{
    private const float MIN_DIST_TEMPORARY = 5;
    
    public static IEnumerable<SoundRequest> GetAllRequestedBelowZeroSounds()
    {
        var set = new List<SoundRequest>();
        foreach (Type type in Plugin.Assembly.GetTypes())
        {
            var attributes = type.GetCustomAttributes(typeof(RequestBzSoundsAttribute), false);
            foreach (var attribute in attributes)
            {
                if (attribute is not RequestBzSoundsAttribute soundRequest) continue;
                
                foreach (var soundName in soundRequest.SoundNames)
                    set.Add(new SoundRequest
                    {
                        EventName = soundName,
                        MinDistance = MIN_DIST_TEMPORARY,
                        MaxDistance = soundRequest.MaxDistance,
                    });
            }
        }
        return set;
    }
}