using Barotrauma;
using Barotrauma.Plugins;
using Microsoft.Xna.Framework;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

[assembly: IgnoresAccessChecksTo("Barotrauma")]
[assembly: IgnoresAccessChecksTo("DedicatedServer")]
[assembly: IgnoresAccessChecksTo("BarotraumaCore")]

namespace Barotrauma.LuaCs;

public partial class Plugin : IBarotraumaPlugin
{
    public void Init()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        string assemblyPath = assembly.Location;
        AssemblyLoadContext alc = AssemblyLoadContext.GetLoadContext(assembly)!;

        if (alc != AssemblyLoadContext.Default)
        {
            if (AssemblyLoadContext.Default.Assemblies.Any(ass => ass.GetName().Name == assembly.GetName().Name))
            {
                // Don't load twice
                return;
            }

            // Copy LuaCs dlls to temp folder
            string destination = Path.GetFullPath("./LuaCs.Temp");

            Directory.CreateDirectory(destination);

            foreach (string file in Directory.GetFiles(Path.GetDirectoryName(assemblyPath)!))
            {
                string destFile = Path.Combine(destination, Path.GetFileName(file));
                File.Copy(file, destFile, overwrite: true);
            }

            // Load assembly in new location
            Assembly newAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath($"{Path.Combine(destination, Path.GetFileName(assemblyPath))}");

            IBarotraumaPlugin pluginObj = (IBarotraumaPlugin)Activator.CreateInstance(newAssembly.GetType("Barotrauma.LuaCs.Plugin")!)!;

            pluginObj.Init();

            return;
        }

        // Just so default context knows how to load our dependencies
        var resolver = new AssemblyDependencyResolver(assemblyPath);

        AssemblyLoadContext.Default.Resolving += (context, assemblyName) =>
        {
            string? path = resolver.ResolveAssemblyToPath(assemblyName);

            if (path != null)
            {
                return context.LoadFromAssemblyPath(path);
            }

            return null;
        };

        DebugConsole.NewMessage("LuaCsForBarotrauma loaded", Color.Lime);

        InitProjectSpecific();

        LuaCsSetup.Instance.GetType();
    }

    public partial void InitProjectSpecific();

    public void Dispose() 
    {
        DebugConsole.NewMessage("LuaCsForBarotrauma unloaded", Color.Red); 
    }

    public void OnContentLoaded()
    {
        
    }
}