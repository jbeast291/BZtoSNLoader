using BepInEx.Logging;

namespace ComponentPortFramework;

public interface ILogger
{
    public void Log(LogLevel severity, string message);
}