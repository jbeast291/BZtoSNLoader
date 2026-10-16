namespace ComponentPortFramework;

public class PrefabComponentSerializerSettings
{
    /// <summary>
    /// Enables various hardcoded fixes that help in reconstructing creatures. 
    /// </summary>
    public bool EnableCreatureFixes { get; init; }

    /// <summary>
    /// If true, enables all behavior components that can be toggled on/off. 
    /// </summary>
    public bool EnableAllComponents { get; init; } = true;

    public bool SkipFmodComponents { get; init; } = true;

    public int RecursionDepthLimit { get; init; } = 50;
}