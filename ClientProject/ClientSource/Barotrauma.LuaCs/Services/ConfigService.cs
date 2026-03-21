using System;
using System.Collections.Immutable;
using System.Linq;
using Barotrauma.LuaCs.Configuration;
using Barotrauma.LuaCs.Data;
using Barotrauma.Networking;
using FluentResults;

namespace Barotrauma.LuaCs;

public sealed partial class ConfigService
{
    public ImmutableArray<ISettingBase> GetDisplayableConfigs()
    {
        using var _ = _operationLock.AcquireReaderLock().ConfigureAwait(false).GetAwaiter().GetResult();
        IService.CheckDisposed(this);

        return _settingsInstances.Values
            .Where(s => !s.IsDisposed)
            .Where(s => s.GetDisplayInfo().ShowInMenus)
            .ToImmutableArray();
    }
}
