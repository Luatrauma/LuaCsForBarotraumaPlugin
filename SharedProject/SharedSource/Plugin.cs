using Barotrauma;
using Barotrauma.Plugins;
using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;

namespace LuaCsForBarotrauma;

public partial class Plugin : IBarotraumaPlugin
{
    public static readonly IDebugConsole DebugConsole = PluginServiceProvider.GetService<IDebugConsole>();
    private Script? script;

    public void Init()
    { 
        DebugConsole.NewMessage("LuaCsForBarotrauma loaded", Color.Lime);

        LuaCsSetup.Instance.GetType();
    }

    public void Dispose() 
    { 
        script = null;
        DebugConsole.NewMessage("LuaCsForBarotrauma unloaded", Color.Red);
    }

    public void OnContentLoaded()
    {
        
    }
}