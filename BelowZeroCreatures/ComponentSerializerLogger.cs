using BepInEx.Logging;
using ComponentPortFramework;

namespace BelowZeroCreatures;

public class ComponentSerializerLogger : ILogger
{
    public void Log(LogLevel severity, string message)
    {
        Plugin.Logger.Log(severity, $"[Component Serializer] {message}");
    }
}