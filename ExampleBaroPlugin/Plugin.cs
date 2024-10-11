using BarotraumaModInterface;

public class Plugin : IBarotraumaPlugin
{
    [PluginService]
    public static IDebugConsole DebugConsole { get; private set; } = null!;

    public void Init()
    {
        DebugConsole.NewMessage("Plugin loaded");
    }

    public void Dispose()
    {
        DebugConsole.NewMessage("Plugin unloaded");
    }
}