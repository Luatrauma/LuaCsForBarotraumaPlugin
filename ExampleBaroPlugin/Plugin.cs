using Barotrauma.Plugins;
using Microsoft.Xna.Framework;

public class Plugin : IBarotraumaPlugin
{
    public static readonly IDebugConsole DebugConsole = PluginServiceProvider.GetService<IDebugConsole>();

    public void Init()
    {
        DebugConsole.NewMessage("Plugin loaded", Color.Lime);
        DebugConsole.RegisterCommand(
            command: "test",
            helpMessage: "An example command from a mod.",
            flags: CommandFlags.DoNotRelayToServer,
            onCommandExecuted: OnTestCommandExecuted);
    }

    private void OnTestCommandExecuted(string[] args)
    {
        DebugConsole.NewMessage("Hello, World!");
    }

    public void Dispose()
    {
        DebugConsole.NewMessage("Plugin unloaded", Color.Red);
    }
}