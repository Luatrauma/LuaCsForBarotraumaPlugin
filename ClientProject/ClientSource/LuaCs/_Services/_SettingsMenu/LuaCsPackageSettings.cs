using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.LuaCs.Data;
using Barotrauma.Networking;
using Barotrauma.Plugins;
using Microsoft.Xna.Framework;

namespace Barotrauma.LuaCs;

internal sealed class LuaCsPackageSettings : ISetting
{
    private readonly ImmutableArray<ISettingBase> _configs;
    private readonly IConfigService _configService;
    private readonly Dictionary<ISettingBase, string> _pendingValues = new();
    private GUIFrame? _content;

    public LuaCsPackageSettings(ContentPackage package, IEnumerable<ISettingBase> configs, IConfigService configService)
    {
        Identifier = package.Name.ToIdentifier();
        _configService = configService;
        _configs = configs
            .OrderBy(s => GetCategory(s))
            .ThenBy(s => GetName(s))
            .ToImmutableArray();
    }

    public Identifier Identifier { get; }

    public SettingSyncMode SyncMode => SettingSyncMode.NoSync;

    public SettingIsAllowedToSetDelegate IsAllowedToSet { get; } = (_, _) => true;

    public bool HasPendingChanges => _pendingValues.Count > 0;

    public bool IsDirty { get; set; }

    public bool ShowInUI { get; set; } = true;

    public void ResetToDefault()
    {
        _pendingValues.Clear();

        foreach (var config in _configs)
        {
            SetAndSave(config, config.GetDefaultStringValue());
        }

        if (_content != null)
        {
            _content.ClearChildren();
            AddUI(_content);
        }
    }

    public void Apply()
    {
        foreach (var (config, value) in _pendingValues)
        {
            SetAndSave(config, value);
        }

        _pendingValues.Clear();
    }

    public void Revert()
    {
        _pendingValues.Clear();
    }

    public bool ShouldSave() => false;

    public void Load(XElement element) { }

    public void Save(XElement element) { }

    public void WriteMessage(IWriteMessage writeMessage) { }

    public void ReadMessage(IReadMessage readMessage) { }

    public void AddUI(GUIFrame content)
    {
        _content = content;

        string? lastCategory = null;

        foreach (var config in _configs)
        {
            string category = GetCategory(config);

            if (category != lastCategory)
            {
                lastCategory = category;
                new GUITextBlock(new RectTransform((1f, 0.06f), content.RectTransform), category, font: GUIStyle.SubHeadingFont);
            }

            var info = config.GetDisplayInfo();

            var row = new GUIFrame(new RectTransform((1f, 0.07f), content.RectTransform), style: null);
            var layout = new GUILayoutGroup(new RectTransform(Vector2.One, row.RectTransform), isHorizontal: true);

            new GUITextBlock(new RectTransform((0.6f, 1f), layout.RectTransform), GetName(config), font: GUIStyle.SmallFont, textAlignment: Alignment.Left)
            {
                ToolTip = Localize(info.Tooltip, string.Empty)
            };

            config.AddDisplayComponent(layout, new Vector2(0.4f, 1f), newValue => _pendingValues[config] = newValue);
        }
    }

    private void SetAndSave(ISettingBase config, string value)
    {
        if (config.IsDisposed || !config.TrySetSerializedValue(value))
        {
            return;
        }

        _configService.SaveConfigValue(config);
    }

    private static string GetName(ISettingBase config) => Localize(config.GetDisplayInfo().DisplayName, config.InternalName);

    private static string GetCategory(ISettingBase config) => Localize(config.GetDisplayInfo().DisplayCategory, "General");

    private static string Localize(string token, string fallback)
    {
        var text = TextManager.Get(token);

        return text.IsNullOrWhiteSpace() ? fallback : text.Value;
    }
}
