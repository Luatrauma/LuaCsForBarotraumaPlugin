using Barotrauma;
using Barotrauma.Plugins;
using ClientSource;
using Microsoft.Xna.Framework;

public partial class Plugin
{
    public partial void InitProjectSpecific()
    {
        CampaignLifecycle.RegisterOnCampaignMapUpdate(OnCampaignUpdate);
    }
    
    bool wasAltDown = false;
    bool isAltDown = false;

    private void OnCampaignUpdate(CampaignMode mode, Map map, float deltaTime)
    {
        wasAltDown = isAltDown;
        isAltDown = PlayerInput.IsAltDown();

        if (!wasAltDown || isAltDown) { return; }

        ShortcutManager shortcutManager = mode.Map.GetExtraField<ShortcutManager>(ShortcutManagerField) ?? throw new InvalidOperationException("ShortcutManager not found in map extra fields.");

        Location? hoveredLocation = mode.Map.HighlightedLocation;

        if (hoveredLocation is null) { return; }

        bool hasShortcut = shortcutManager.HasShortcut(mode.Map.CurrentLocation, hoveredLocation);

        GUIContextMenu.CreateContextMenu(new ContextMenuOption("Create shortcut", !hasShortcut, () =>
                                         {
                                             DebugConsole.NewMessage($"Created shortcut between {mode.Map.CurrentLocation.DisplayName} and {hoveredLocation.DisplayName}", Color.Lime);
                                             shortcutManager.AddShortcut(mode.Map.CurrentLocation, hoveredLocation);
                                         }),
                                         new ContextMenuOption("Traverse", hasShortcut, () =>
                                         {
                                             DebugConsole.NewMessage($"Traversing from {mode.Map.CurrentLocation.DisplayName} to {hoveredLocation.DisplayName}", Color.Azure);
                                             mode.Map.SelectLocation(hoveredLocation);
                                         }));
    }
}