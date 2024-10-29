using System.Text;
using Barotrauma.Plugins;
using ClientSource;
using Microsoft.Xna.Framework;

public class Plugin : IBarotraumaPlugin
{
    public static readonly IDebugConsole DebugConsole = PluginServiceProvider.GetService<IDebugConsole>();
    public static readonly IContentPackage ContentPackage = PluginServiceProvider.GetService<IContentPackage>();

    public void Init()
    {
        DebugConsole.NewMessage("Plugin loaded test", Color.Lime);
        DebugConsole.RegisterCommand(
            command: "listprefabs",
            helpMessage: "List prefabs created by the this content package.",
            flags: CommandFlags.DoNotRelayToServer,
            onCommandExecuted: OnTestCommandExecuted);

        ContentPackage.RegisterContentPackage<MyContentFile>();
    }

    private void OnTestCommandExecuted(string[] args)
    {
        StringBuilder sb = new("Prefabs created by this content package:\n");
        foreach (MyPrefab prefab in MyPrefab.Prefabs)
        {
            sb.AppendLine($"    - {prefab.Identifier} MyProperty: {prefab.MyProperty}");
        }

        DebugConsole.NewMessage(sb.ToString(), Color.White);
    }

    public void Dispose()
    {
        DebugConsole.NewMessage("Plugin unloaded", Color.Red);
    }
}