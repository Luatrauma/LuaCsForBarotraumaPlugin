using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Plugins;
using HarmonyLib;

namespace Barotrauma.LuaCs;

public class SettingsMenuSystem : ISettingsMenuSystem
{
    private readonly Harmony _harmony;
    private readonly IConfigService _configService;
    private readonly ILoggerService _loggerService;
    private readonly List<SettingsService> _services = new();
    private static SettingsMenuSystem SystemInstance;

    public SettingsMenuSystem(IConfigService configService, ILoggerService loggerService)
    {
        _configService = configService;
        _loggerService = loggerService;
        SystemInstance = this;
        _harmony = Harmony.CreateAndPatchAll(typeof(SettingsMenuSystem));
    }

    [HarmonyPatch(typeof(SettingsMenu), nameof(SettingsMenu.RecreateModSettingsTab)), HarmonyPrefix]
    private static void SettingsMenu_RecreateModSettingsTab_Pre()
    {
        SystemInstance?.SyncServices();
    }

    [HarmonyPatch(typeof(SettingsService), nameof(SettingsService.Load)), HarmonyPrefix]
    private static bool SettingsService_Load_Pre(SettingsService __instance) => !IsOwnService(__instance);

    [HarmonyPatch(typeof(SettingsService), nameof(SettingsService.Save)), HarmonyPrefix]
    private static bool SettingsService_Save_Pre(SettingsService __instance) => !IsOwnService(__instance);

    private static bool IsOwnService(SettingsService service) => SystemInstance != null && SystemInstance._services.Contains(service);

    private void SyncServices()
    {
        RemoveServices();

        try
        {
            var displayableConfigs = _configService.GetDisplayableConfigs();

            _loggerService.LogMessage($"Mod settings menu: {displayableConfigs.Length} displayable LuaCs configs");

            foreach (var packageConfigs in displayableConfigs.GroupBy(s => s.OwnerPackage))
            {
                ContentPackage package = packageConfigs.Key;

                if (package == null)
                {
                    continue;
                }

                _loggerService.LogMessage($"Mod settings menu: {package.Name} has {packageConfigs.Count()} configs, enabled: {ContentPackageManager.EnabledPackages.All.Contains(package)}");

                var service = new SettingsService(new PluginData(Plugin.OwnAssembly, package), Plugin.OwnAssembly);
                service.RegisterSetting(new LuaCsPackageSettings(package, packageConfigs, _configService));

                _services.Add(service);
            }
        }
        catch (Exception e)
        {
            _loggerService.LogError($"Failed to add LuaCs settings to the mod settings menu: {e}");
        }
    }

    private void RemoveServices()
    {
        foreach (var service in _services)
        {
            PluginData.SettingsServices.Remove(service);
        }

        _services.Clear();
    }

    #region DISPOSAL

    public void Dispose()
    {
        if (!ModUtils.Threading.CheckIfClearAndSetBool(ref _isDisposed))
        {
            return;
        }
        _harmony.UnpatchSelf();
        RemoveServices();
        GC.SuppressFinalize(this);
    }
    private int _isDisposed = 0;
    public bool IsDisposed
    {
        get => ModUtils.Threading.GetBool(ref _isDisposed);
        private set => ModUtils.Threading.SetBool(ref _isDisposed, value);
    }
    public FluentResults.Result Reset()
    {
        throw new NotImplementedException();
    }

    #endregion
}
