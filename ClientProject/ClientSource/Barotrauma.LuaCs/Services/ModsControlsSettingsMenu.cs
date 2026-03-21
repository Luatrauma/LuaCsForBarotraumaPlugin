namespace Barotrauma.LuaCs;

internal sealed class ModsControlsSettingsMenu : ModsSettingsMenu
{
    public ModsControlsSettingsMenu(GUIFrame contentFrame, 
        IPackageManagementService packageManagementService, 
        IConfigService configService, 
        SettingsMenu settingsMenuInstance) : base(contentFrame, packageManagementService, configService, settingsMenuInstance)
    {
        
    }

    protected override void DisposeInternal()
    {
        // TODO: Finish this later.
    }
}
