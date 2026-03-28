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
        ContentPackage package = LuaCsSetup.GetLuaCsPackage();
        if (PluginLoader.IsPluginFileUnloading(package.GetFiles<PluginInfoFile>().First()))
        {
            DebugConsole.NewMessage("Detected that LuaCsForBarotrauma is still loaded, skipping load so we don't load twice", Color.Lime);
            return;
        }

        DebugConsole.NewMessage("LuaCsForBarotrauma loaded", Color.Lime);

        InitProjectSpecific();

        LuaCsSetup.Instance.GetType();
    }

    public partial void InitProjectSpecific();

    public void Dispose() 
    {
        //LuaCsSetup.Instance.Dispose();

        DebugConsole.NewMessage("LuaCsForBarotrauma unloaded", Color.Red); 
    }

    public void OnContentLoaded()
    {
        
    }
}