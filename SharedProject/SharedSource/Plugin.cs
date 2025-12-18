using Barotrauma;
using Barotrauma.Plugins;
using Microsoft.Xna.Framework;

namespace ExampleMod;

public partial class Plugin : IBarotraumaPlugin
{
    public static readonly IDebugConsole DebugConsole = PluginServiceProvider.GetService<IDebugConsole>();

    public void Init()
    {
        DebugConsole.NewMessage("Plugin loaded", Color.Lime);

        InitProjectSpecific();
    }

    public partial void InitProjectSpecific();

    public void Dispose() 
    { 
        DebugConsole.NewMessage("Plugin unloaded", Color.Red); 
    }

    public void OnContentLoaded()
    {
        
    }
}