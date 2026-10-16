using BepInEx.Logging;
using ComponentPortFramework;

namespace BZPrefabReader;

public class FrameworkLogger : ILogger
{
    public void Log(LogLevel severity, string message)
    {
        Plugin.Logger.Log(severity, $"[Component Framework] {message}");
    }
}