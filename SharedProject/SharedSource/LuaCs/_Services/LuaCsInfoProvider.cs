using System;
using System.Collections.Generic;
using System.Linq;

namespace Barotrauma.LuaCs;

public sealed class LuaCsInfoProvider : ILuaCsInfoProvider
{
    public void Dispose()
    {
        // stateless service
    }

    public bool IsDisposed => false;
    public bool IsCsEnabled => LuaCsSetup.Instance.IsCsEnabled;
    public bool DisableErrorGUIOverlay => LuaCsSetup.Instance.DisableErrorGUIOverlay;
    public bool HideUserNamesInLogs => LuaCsSetup.Instance.HideUserNamesInLogs;
    public ulong LuaForBarotraumaSteamId => LuaCsSetup.Instance.LuaForBarotraumaSteamId;
    public bool RestrictMessageSize => LuaCsSetup.Instance.RestrictMessageSize;
    public string LocalDataSavePath => LuaCsSetup.Instance.LocalDataSavePath;
    public RunState CurrentRunState => LuaCsSetup.Instance.CurrentRunState;
    public ContentPackage LuaCsForBarotraumaPackage
    {
        get
        {
            var luaCs = FirstOrDefaultLua(ContentPackageManager.EnabledPackages.All);
            if (luaCs == null)
            {
                luaCs = FirstOrDefaultLua(ContentPackageManager.LocalPackages.Regular);
            }

            if (luaCs == null)
            {
                luaCs = FirstOrDefaultLua(ContentPackageManager.WorkshopPackages.Regular);
            }
            
            return luaCs;

            ContentPackage FirstOrDefaultLua(IEnumerable<ContentPackage> packages)
            {
                return packages.FirstOrDefault(p =>
                    p.Name.Equals("LuaCsForBarotrauma", StringComparison.InvariantCultureIgnoreCase)
                    || p.Name.Equals("Lua for Barotrauma", StringComparison.InvariantCultureIgnoreCase));
            }
        }
    }
}
