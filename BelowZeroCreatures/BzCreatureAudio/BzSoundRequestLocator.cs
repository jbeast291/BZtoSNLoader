using System;
using System.Collections.Generic;

namespace BelowZeroCreatures.BzCreatureAudio;

internal static class BzSoundRequestLocator
{
    private const float MIN_DIST_TEMPORARY = 5;
    
    public static IEnumerable<(string, float, float)> GetAllRequestedBelowZeroSounds()
    {
        var set = new List<(string, float, float)>();
        foreach (Type type in Plugin.Assembly.GetTypes())
        {
            var attributes = type.GetCustomAttributes(typeof(RequestBzSoundsAttribute), false);
            foreach (var attribute in attributes)
            {
                if (attribute is not RequestBzSoundsAttribute soundRequest) continue;
                
                foreach (var soundName in soundRequest.SoundNames)
                    set.Add((soundName, MIN_DIST_TEMPORARY, soundRequest.MaxDistance));
            }
        }
        return set;
    }
}