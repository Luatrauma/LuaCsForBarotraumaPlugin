using Barotrauma;
using Barotrauma.Plugins;
using HarmonyLib;
using Microsoft.Xna.Framework;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

[assembly: IgnoresAccessChecksTo("Barotrauma")]
[assembly: IgnoresAccessChecksTo("DedicatedServer")]
[assembly: IgnoresAccessChecksTo("BarotraumaCore")]

namespace Barotrauma.LuaCs;

public partial class Plugin : IBarotraumaPlugin
{
    private static IStatusEffectService? _statusEffectService;
    private static IDebugConsole? _debugConsole;

    public static IStatusEffectService StatusEffectService => _statusEffectService ??= PluginServiceProvider.GetService<IStatusEffectService>();
    public static IDebugConsole DebugConsole => _debugConsole ??= PluginServiceProvider.GetService<IDebugConsole>();

    private static bool _initialized;
    private static Assembly? _ownAssembly;
    private static PluginData? _ownData;
    private static PluginMetadata _ownMetadata;
    internal static Assembly OwnAssembly => _ownAssembly!;

    public void Init()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        string assemblyPath = assembly.Location;
        AssemblyLoadContext alc = AssemblyLoadContext.GetLoadContext(assembly)!;

        if (alc != AssemblyLoadContext.Default)
        {
            Assembly? newAssembly = AssemblyLoadContext.Default.Assemblies.FirstOrDefault(ass => ass.GetName().Name == assembly.GetName().Name);

            if (newAssembly == null)
            {
                // Copy LuaCs dlls to temp folder
                string destination = Path.GetFullPath("./LuaCs.Temp");

#if SERVER
                destination = Path.Combine(destination, "Server");
#elif CLIENT
                destination = Path.Combine(destination, "Client");
#endif

                Directory.CreateDirectory(destination);

                foreach (string file in Directory.GetFiles(Path.GetDirectoryName(assemblyPath)!))
                {
                    string destFile = Path.Combine(destination, Path.GetFileName(file));
                    File.Copy(file, destFile, overwrite: true);
                }

                // Load assembly in new location
                newAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath($"{Path.Combine(destination, Path.GetFileName(assemblyPath))}");

                IBarotraumaPlugin pluginObj = (IBarotraumaPlugin)Activator.CreateInstance(newAssembly.GetType("Barotrauma.LuaCs.Plugin")!)!;

                pluginObj.Init();
            }

            return;
        }

        // From here on we are the assembly living in the default context
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        InstallAssemblyBridge(assembly);

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

#if CLIENT
        if (Screen.Selected != null)
        {
            LuaCsSetup.Instance.OnScreenSelected(Screen.Selected);
        }
#endif
    }

    private static void InstallAssemblyBridge(Assembly ownAssembly)
    {
        _ownAssembly = ownAssembly;

        // Try to get the package that loaded our plugin
        var loaderPlugin = PluginLoader.LoadedPlugins
            .FirstOrDefault(lp => lp.Assembly?.GetName().Name == ownAssembly.GetName().Name);

        ContentPackage contentPackage = loaderPlugin.Data?.ContentPackage ?? LuaCsSetup.GetLuaCsPackage();
        _ownMetadata = loaderPlugin.Metadata;

        _ownData = new PluginData(ownAssembly, contentPackage);

        var harmony = new Harmony("luacs.assembly-bridge");

        // Patch Barotrauma methods to inject our own PluginData, since we are not actually a real plugin now

        harmony.Patch(AccessTools.Method(typeof(PluginLoader), "TryFindPluginDataForAssembly"),
            postfix: new HarmonyMethod(typeof(Plugin), nameof(TryFindPluginDataPostfix)));
        harmony.Patch(AccessTools.Method(typeof(PluginLoader), "AddTransientService"),
            prefix: new HarmonyMethod(typeof(Plugin), nameof(AddTransientServicePrefix)));
        harmony.Patch(AccessTools.Method(typeof(PluginLoader), "ReconstructPluginDataList"),
            postfix: new HarmonyMethod(typeof(Plugin), nameof(ReconstructPluginDataListPostfix)));
        harmony.Patch(AccessTools.Method(typeof(PluginLoader), "TryFindMetadataForAssembly"),
            postfix: new HarmonyMethod(typeof(Plugin), nameof(TryFindMetadataPostfix)));

        ReconstructPluginDataListPostfix();
    }

    private static void TryFindMetadataPostfix(Assembly assembly, ref Option<PluginMetadata> __result)
    {
        if (assembly == _ownAssembly && !__result.TryUnwrap(out _))
        {
            __result = Option.Some(_ownMetadata);
        }
    }

    private static void TryFindPluginDataPostfix(Assembly assembly, ref PluginData? __result)
    {
        if (__result == null && assembly == _ownAssembly)
        {
            __result = _ownData;
        }
    }

    private static readonly List<ITransientService> _ownTransientServices = new();
    private static bool AddTransientServicePrefix(Assembly assembly, ITransientService service)
    {
        if (assembly != _ownAssembly)
        {
            return true;
        }

        _ownTransientServices.Add(service);
        return false;
    }

    private static void ReconstructPluginDataListPostfix()
    {
        if (_ownData == null || PluginData.LoadedPluginData.Contains(_ownData))
        {
            return;
        }

        PluginData.LoadedPluginData = PluginData.LoadedPluginData.Add(_ownData);
        PluginData.cacheInvalidated = true;
    }

    public partial void InitProjectSpecific();

    public void Dispose()
    {

    }

    public void OnContentLoaded()
    {

    }
}
