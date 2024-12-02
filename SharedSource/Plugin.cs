using System.Text;
using System.Xml.Linq;
using Barotrauma;
using Barotrauma.Plugins;
using ClientSource;
using Microsoft.Xna.Framework;

public partial class Plugin : IBarotraumaPlugin
{
    public static readonly IDebugConsole DebugConsole = PluginServiceProvider.GetService<IDebugConsole>();
    public static readonly IContentPackage ContentPackage = PluginServiceProvider.GetService<IContentPackage>();
    public static readonly ICampaignLifecycle CampaignLifecycle = PluginServiceProvider.GetService<ICampaignLifecycle>();

    public void Init()
    {
        DebugConsole.NewMessage("Plugin loaded test", Color.Lime);
        DebugConsole.RegisterCommand(
            command: "listprefabs",
            helpMessage: "List prefabs created by the this content package.",
            flags: CommandFlags.DoNotRelayToServer,
            onCommandExecuted: OnTestCommandExecuted);

        ContentPackage.RegisterContentPackage<MyContentFile>();

        CampaignLifecycle.RegisterOnCampaignLoad(OnCampaignLoad);
        CampaignLifecycle.RegisterOnCampaignSave(OnCampaignSave);

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

    public void Dispose()
    {
        DebugConsole.NewMessage("Plugin unloaded", Color.Red);
    }
}