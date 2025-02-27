using System.Diagnostics;
using Barotrauma;
using Barotrauma.Plugins;
using ClientSource;
using Microsoft.Xna.Framework;

namespace ExampleMod;

public partial class Plugin
{
    public partial void InitProjectSpecific()
    {
        HookService.RegisterHook<PluginCampaignMapUpdateDelegate>(OnCampaignUpdate);
        GameNetwork.RegisterHandler<NetworkHeaders, EchoRequestResponse>(NetworkHeaders.RespondEcho, HandleBar);
        DebugConsole.RegisterCommand(command: "testnetwork",
                                     helpMessage: "Send a network message to the server",
                                     flags: CommandFlags.DoNotRelayToServer,
                                     onCommandExecuted: static (string[] args) =>
                                     {
                                         Random random = new Random();
                                         GameNetwork.Send(NetworkHeaders.RequestEcho, new EchoRequestData("Test".ToIdentifier(), random.Next()));
                                     });

        DebugConsole.RegisterCommand(command: "testneworkrequestentityevents",
                                     helpMessage: "Make the server create entity events for specific items based on the identifier",
                                     flags: CommandFlags.DoNotRelayToServer,
                                     onCommandExecuted: static (string[] args) =>
                                     {
                                         GameNetwork.Send(NetworkHeaders.RequestEntityEvents, new ClientRequestEntityEventData(args.Select(static a => a.ToIdentifier()).ToArray()));
                                     });
    }

    private void HandleBar(EchoRequestResponse data)
    {
        DebugConsole.NewMessage($"Received BarData with time: {data.DateTime} and position: {data.Vec2.Fallback(Vector2.Zero)}");
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