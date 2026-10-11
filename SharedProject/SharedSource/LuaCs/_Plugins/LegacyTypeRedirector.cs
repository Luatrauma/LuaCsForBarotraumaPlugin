using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;

namespace Barotrauma.LuaCs;

/// <summary>
/// Rewrites old assembly mods referencing Barotrauma.LuaCs to reference the new plugin assembly.
/// </summary>
internal static class LegacyTypeRedirector
{
    private static readonly string[] GameAssemblyNames = { "Barotrauma", "BarotraumaCore", "DedicatedServer" };

    /// <summary>
    /// Returns the rewritten assembly bytes, or null if nothing had to be redirected.
    /// </summary>
    public static byte[]? TryRedirect(string assemblyPath)
    {
        using var module = ModuleDefinition.ReadModule(assemblyPath, new ReaderParameters { InMemory = true });

        Assembly luaCsAssembly = typeof(LuaCsSetup).Assembly;
        AssemblyName luaCsName = luaCsAssembly.GetName();
        AssemblyNameReference? luaCsReference = null;
        bool changed = false;

        foreach (TypeReference typeRef in module.GetTypeReferences().ToList())
        {
            if (typeRef.DeclaringType != null || typeRef.Scope is not AssemblyNameReference scope || !GameAssemblyNames.Contains(scope.Name))
            {
                continue;
            }

            Assembly? gameAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == scope.Name);

            if (gameAssembly == null || gameAssembly.GetType(typeRef.FullName) != null || luaCsAssembly.GetType(typeRef.FullName) == null)
            {
                continue;
            }

            luaCsReference ??= module.AssemblyReferences.FirstOrDefault(r => r.Name == luaCsName.Name) ?? AddReference(module, luaCsName);

            typeRef.Scope = luaCsReference;
            changed = true;
        }

        if (!changed)
        {
            return null;
        }

        using var stream = new MemoryStream();
        module.Write(stream);
        return stream.ToArray();
    }

    private static AssemblyNameReference AddReference(ModuleDefinition module, AssemblyName name)
    {
        var reference = new AssemblyNameReference(name.Name, name.Version)
        {
            PublicKeyToken = name.GetPublicKeyToken()
        };

        module.AssemblyReferences.Add(reference);

        return reference;
    }
}
