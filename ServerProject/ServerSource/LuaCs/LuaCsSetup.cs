using System;
using System.IO;
using Barotrauma.Networking;
using HarmonyLib;

namespace Barotrauma;

partial class LuaCsSetup
{
    partial void CheckReadyToRun(Action onReadyToRun)
    {
        onReadyToRun?.Invoke();
    }

    internal void ApplyServerListVisibility()
    {
        if (GetLuaCsPackage() is not { } luaCs || ReadHideFromServerListSetting(luaCs))
        {
            return;
        }

        // hack
        AccessTools.Field(typeof(ContentPackage), $"<{nameof(ContentPackage.HasMultiplayerSyncedContent)}>k__BackingField").SetValue(luaCs, true);
    }
    
    /// <summary>
    /// Handles changes in game states tracked by screen changes.
    /// </summary>
    /// <param name="screen">The new game screen.</param>
    public partial void OnScreenSelected(Screen screen)
    {
        // the server is always in the running state unless explicitly stopped.
        if (screen == UnimplementedScreen.Instance)
            SetRunState(RunState.Unloaded);
        SetRunState(RunState.Running);
    }
}
