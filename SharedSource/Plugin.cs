using System.Text;
using System.Xml.Linq;
using Barotrauma;
using Barotrauma.Plugins;
using ClientSource;
using Microsoft.Xna.Framework;

namespace ExampleMod;

public partial class Plugin : IBarotraumaPlugin
{
    public static readonly IDebugConsole DebugConsole = PluginServiceProvider.GetService<IDebugConsole>();
    public static readonly IContentFileRegistrar ContentFileRegistrar = PluginServiceProvider.GetService<IContentFileRegistrar>();
    public static readonly ISimpleHookService HookService = PluginServiceProvider.GetService<ISimpleHookService>();
    public static readonly IItemComponentRegistrar ItemComponentRegistrar = PluginServiceProvider.GetService<IItemComponentRegistrar>();
    public static readonly IGameNetwork GameNetwork = PluginServiceProvider.GetService<IGameNetwork>();

    public enum NetworkHeaders
    {
        RequestEcho,
        RespondEcho,
        RequestEntityEvents
    }

    public enum EntityEventType
    {
        SendHealthAndIdentifier
    }

    [NetworkSerialize]
    public readonly record struct EchoRequestData(Identifier TestIdentifier, int TestInt) : INetSerializableStruct;

    [NetworkSerialize]
    public readonly record struct EchoRequestResponse(SerializableDateTime DateTime, Option<Vector2> Vec2) : INetSerializableStruct;

    [NetworkSerialize]
    public readonly record struct ClientRequestEntityEventData(Identifier[] Identifier) : INetSerializableStruct;

    public void Init()
    {
        DebugConsole.NewMessage("Plugin loaded test", Color.Lime);
        DebugConsole.RegisterCommand(
            command: "listprefabs",
            helpMessage: "List prefabs created by the this content package.",
            flags: CommandFlags.DoNotRelayToServer,
            onCommandExecuted: OnTestCommandExecuted);

        ContentFileRegistrar.RegisterContentFile<MyContentFile>();

        HookService.RegisterHook<PluginCampaignLoadDelegate>(OnCampaignLoad);
        HookService.RegisterHook<PluginCampaignSaveDelegate>(OnCampaignSave);

        // test that this doesn't cause unload issues
        GameMain.SubEditorScreen.GetOrAddExtraField("UnloadTest", new object());

        ItemComponentRegistrar.RegisterItemComponent<MyItemComponent>();

        GameNetwork.RegisterNetworkHeaders<NetworkHeaders>();
        GameNetwork.RegisterCustomEntityEventHandler(new ItemEntityEventHandler());

        InitProjectSpecific();
    }

    public partial void InitProjectSpecific();

    public const string ShortcutManagerField = "ShortcutManager";

    private void OnCampaignLoad(CampaignMode campaign, Option<XElement> saveFile, CampaignSettings settings)
    {
        ShortcutManager shortcutManager = saveFile.TryUnwrap(out XElement? element)
                                              ? new ShortcutManager(campaign, element)
                                              : new ShortcutManager(campaign);

        campaign.Map.SetExtraField(ShortcutManagerField, shortcutManager);
    }

    private void OnCampaignSave(CampaignMode mode, XElement save)
    {
        ShortcutManager? shortcutManager = mode.Map.GetExtraField<ShortcutManager>(ShortcutManagerField);
        shortcutManager?.Save(save);
    }

    private void OnTestCommandExecuted(string[] args)
    {
        StringBuilder sb = new("Prefabs created by this content package:\n");
        foreach (MyPrefab prefab in MyPrefab.Prefabs)
        {
            sb.AppendLine($"    - {prefab.Identifier} MyProperty: {prefab.MyProperty}");
        }

        DebugConsole.NewMessage(sb.ToString(), Color.White);
    }

    public void Dispose() { DebugConsole.NewMessage("Plugin unloaded", Color.Red); }
}