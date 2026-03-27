using Barotrauma;
using Barotrauma.Plugins;
using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;

[assembly: IgnoresAccessChecksTo("Barotrauma")]
[assembly: IgnoresAccessChecksTo("DedicatedServer")]
[assembly: IgnoresAccessChecksTo("BarotraumaCore")]

namespace Barotrauma.LuaCs;

public partial class Plugin : IBarotraumaPlugin
{
    public static readonly IDebugConsole DebugConsole = PluginServiceProvider.GetService<IDebugConsole>();

    public void Init()
    {
        DebugConsole.NewMessage("Plugin loaded", Color.Lime);

        InitProjectSpecific();

        LuaCsSetup.Instance.GetType();
    }

    public partial void InitProjectSpecific();

    public void Dispose() 
    {
        //LuaCsSetup.Instance.Dispose();

        DebugConsole.NewMessage("Plugin unloaded", Color.Red); 
    }

    public void OnContentLoaded()
    {
        
    }
}