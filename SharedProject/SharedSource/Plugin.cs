using Barotrauma;
using Barotrauma.Plugins;
using Microsoft.Xna.Framework;

namespace LuaCsForBarotrauma;

public partial class Plugin : IBarotraumaPlugin
{
    public static readonly IDebugConsole DebugConsole = PluginServiceProvider.GetService<IDebugConsole>();

    public void Init()
    {
        DebugConsole.NewMessage("LuaCsForBarotrauma loaded", Color.Lime);
    }

    public void Dispose() 
    { 
        DebugConsole.NewMessage("LuaCsForBarotrauma unloaded", Color.Red); 
    }

    public void OnContentLoaded()
    {
        
    }
}