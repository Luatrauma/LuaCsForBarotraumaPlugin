using System;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma.LuaCs;

internal abstract class ModsSettingsMenu : IDisposable
{
    public GUIFrame ContentFrame { get; private set; }
    protected IPackageManagementService PackageManagementService { get; private set; }
    protected IConfigService ConfigService { get; private set; }
    protected SettingsMenu SettingsMenuInstance { get; private set; }
    
    protected ModsSettingsMenu(GUIFrame contentFrame, 
        IPackageManagementService packageManagementService, 
        IConfigService configService, SettingsMenu settingsMenuInstance)
    {
        ContentFrame = contentFrame;
        PackageManagementService = packageManagementService;
        ConfigService = configService;
        SettingsMenuInstance = settingsMenuInstance;
    }

    protected abstract void DisposeInternal();

    public void Dispose()
    {
        DisposeInternal();
        ContentFrame?.Parent.RemoveChild(ContentFrame);
        SettingsMenuInstance = null;
        ContentFrame = null;
        PackageManagementService = null;
        ConfigService = null;
    }
}
