namespace BelowZeroCreatures.BzCreatureAudio;

public class SoundRequest
{
    public string EventName { get; init; }
    public string[] BzSounds { get; init; }
    public float MinDistance { get; init; }
    public float MaxDistance { get; init; }
}