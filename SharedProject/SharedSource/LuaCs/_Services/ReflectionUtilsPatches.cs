using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace Barotrauma.LuaCs;

[HarmonyPatch]
internal static class ReflectionUtilsPatches
{
    private static readonly ConcurrentDictionary<Assembly, ImmutableArray<Type>> ModAssemblyTypes = new();
    private static readonly ConcurrentDictionary<Type, ImmutableArray<Type>> DerivedTypesCache = new();

    public static void AddNonAbstractAssemblyTypes(Assembly assembly)
    {
        ModAssemblyTypes[assembly] = GetNonAbstractTypes(assembly);
        DerivedTypesCache.Clear();
    }

    public static void RemoveAssemblyFromCache(Assembly assembly)
    {
        ModAssemblyTypes.TryRemove(assembly, out _);
        DerivedTypesCache.Clear();
    }

    public static void ResetCache()
    {
        ModAssemblyTypes.Clear();
        DerivedTypesCache.Clear();
    }

    private static ImmutableArray<Type> GetNonAbstractTypes(Assembly assembly)
    {
        IEnumerable<Type> types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            // Some types failed to load, keep the ones that didn't fail
            types = e.Types.Where(t => t is not null)!;
        }

        return types.Where(t => !t.IsAbstract).ToImmutableArray();
    }

    [HarmonyPatch(typeof(ReflectionUtils), nameof(ReflectionUtils.GetDerivedNonAbstract), new[] { typeof(Type) }), HarmonyPostfix]
    private static void ReflectionUtils_GetDerivedNonAbstract_Post(Type type, ref IEnumerable<Type> __result)
    {
        if (ModAssemblyTypes.IsEmpty) { return; }

        var modTypes = DerivedTypesCache.GetOrAdd(type, baseType => ModAssemblyTypes.Values
            .SelectMany(types => types)
            .Where(t => t.IsSubclassOf(baseType))
            .ToImmutableArray());

        if (modTypes.IsEmpty) { return; }

        __result = __result.Concat(modTypes).Distinct().ToImmutableArray();
    }
}
