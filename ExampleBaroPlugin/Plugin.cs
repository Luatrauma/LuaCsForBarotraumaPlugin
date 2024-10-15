using Barotrauma.Plugins;

public class Plugin : IBarotraumaPlugin
{
    public static readonly IDebugConsole DebugConsole = PluginServiceProvider.GetService<IDebugConsole>();

    public void Init()
    {
        DebugConsole.NewMessage("Plugin loaded");
    }

    public void Dispose()
    {
        DebugConsole.NewMessage("Plugin unloaded");
    }
}