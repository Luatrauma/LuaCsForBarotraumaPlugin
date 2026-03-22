using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Numerics;
using Barotrauma.LuaCs.Data;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector4 = Microsoft.Xna.Framework.Vector4;

// ReSharper disable ObjectCreationAsStatement

namespace Barotrauma.LuaCs;

internal sealed class ModsGameplaySettingsMenu : ModsSettingsMenuBase
{
    private ImmutableArray<ISettingBase> _settingsInstancesGameplay;
    // menu vars
    private GUILayoutGroup _modCategoryDisplayGroup, _settingsDisplayGroup;
    private string _selectedSearchQuery = string.Empty;
    private ContentPackage _selectedContentPackage;
    private string _selectedCategory = string.Empty;

    private event Action OnApplyInstalledModsChanges;
    
    public ModsGameplaySettingsMenu(GUIFrame contentFrame, 
        IPackageManagementService packageManagementService, 
        IConfigService configService, 
        SettingsMenu settingsMenuInstance) : base(contentFrame, packageManagementService, configService, settingsMenuInstance)
    {
        _settingsInstancesGameplay = configService.GetDisplayableConfigs()
            .ToImmutableArray();
        
        
        var mainLayoutGroup = new GUILayoutGroup(new RectTransform(new Vector2(1f, 1f), contentFrame.RectTransform, Anchor.Center), false, Anchor.TopLeft);
        // page title
        var menuTitleLayoutGroup = new GUILayoutGroup(
                new RectTransform(new Vector2(1f, 0.06f), mainLayoutGroup.RectTransform, Anchor.TopLeft), true, Anchor.TopLeft);
        GUIUtil.Label(menuTitleLayoutGroup, "Mods Gameplay Settings", GUIStyle.LargeFont, new Vector2(1f, 1f));
        
        // page contents
        var contentAreaLayoutGroup = new GUILayoutGroup(
                new RectTransform(new Vector2(1f, 0.94f), mainLayoutGroup.RectTransform, Anchor.BottomLeft), false,
                Anchor.TopLeft);
        
        var searchBarLayoutGroup = new GUILayoutGroup(
            new RectTransform(new Vector2(1f, 0.06f), contentAreaLayoutGroup.RectTransform, Anchor.TopCenter), true, Anchor.CenterLeft);
        GUIUtil.Label(searchBarLayoutGroup, "Search: ", GUIStyle.SubHeadingFont, new Vector2(0.1f, 1f));
        var searchBar = new GUITextBox(
            new RectTransform(new Vector2(0.85f, 0.1f), searchBarLayoutGroup.RectTransform, Anchor.TopLeft),
            createClearButton: true)
        {
            OnTextChangedDelegate = (btn, txt) =>
            {
                GenerateDisplayFromFilter(txt);
                return true;
            }
        };
        // main display area
        var settingsContentAreaGroup = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.90f), contentAreaLayoutGroup.RectTransform, Anchor.BottomCenter));
        GUIUtil.Spacer(settingsContentAreaGroup, Vector2.One);
        (_modCategoryDisplayGroup, _settingsDisplayGroup) = GUIUtil.CreateSidebars(settingsContentAreaGroup, true);
        _modCategoryDisplayGroup.RectTransform.RelativeSize = new Vector2(0.3f, 1f);
        _settingsDisplayGroup.RectTransform.RelativeSize = new Vector2(0.7f, 1f);
        
        // default category
        _selectedCategory = "All";

        OnApplyInstalledModsChanges = () =>
        {
            _settingsInstancesGameplay = configService.GetDisplayableConfigs()
                .ToImmutableArray();
            if (_selectedContentPackage is not null && !GetTargetPackagesList().Contains(_selectedContentPackage))
            {
                _selectedContentPackage = null;
                _selectedCategory = string.Empty;
            }
            
            GenerateCategoryListDisplay(_modCategoryDisplayGroup, GetTargetPackagesList(), GetDisplayCategoriesList());
            GenerateSettingsListDisplay(_settingsDisplayGroup, GetDisplaySettingsList());
        };
        
        GenerateCategoryListDisplay(_modCategoryDisplayGroup, GetTargetPackagesList(), GetDisplayCategoriesList());
        GenerateSettingsListDisplay(_settingsDisplayGroup, GetDisplaySettingsList());
        
        void GenerateDisplayFromFilter(string text)
        {
            _selectedSearchQuery = text;
            GenerateCategoryListDisplay(_modCategoryDisplayGroup, GetTargetPackagesList(), GetDisplayCategoriesList());
            GenerateSettingsListDisplay(_settingsDisplayGroup, GetDisplaySettingsList());
        }
        
        string GetLocalizedString(string identifier, string defaultValue)
        {
            var lstr = TextManager.Get(identifier);
            return lstr.IsNullOrWhiteSpace() ? defaultValue : lstr.Value;
        }

        // Filters by selected package and query text
        ImmutableArray<string> GetDisplayCategoriesList()
        {
            return GetFilteredSettingsList()
                .Select(s => GetLocalizedString(s.GetDisplayInfo().DisplayCategory, "General"))
                .Concat(new []{ "All" })
                .Distinct()
                .OrderBy(s => s)
                .ToImmutableArray();
        }

        // Filters by query text
        ImmutableArray<ContentPackage> GetTargetPackagesList()
        {
            return _settingsInstancesGameplay
                .Where(s => SettingMatchesQuery(s, _selectedSearchQuery))
                .Select(s => s.OwnerPackage)
                .Concat(new[] { ContentPackageManager.VanillaCorePackage })
                .Distinct()
                .OrderByDescending(p =>  p == ContentPackageManager.VanillaCorePackage ? 0 : 1)
                .ThenBy(p => p.Name)
                .ToImmutableArray();
        }

        // Filters by selected package, query text, and selected category.
        ImmutableArray<ISettingBase> GetDisplaySettingsList()
        {
            return GetFilteredSettingsList()
                .Where(s => _selectedCategory.IsNullOrWhiteSpace() 
                            || _selectedCategory == "All"
                            || GetLocalizedString(s.GetDisplayInfo().DisplayCategory, "General") == _selectedCategory)
                .OrderBy(s => GetLocalizedString(s.GetDisplayInfo().DisplayName, s.InternalName))
                .ToImmutableArray();
        }

        // Filters by selected package and by query text.
        ImmutableArray<ISettingBase> GetFilteredSettingsList()
        {
            return _settingsInstancesGameplay
                .Where(s => SettingMatchesQuery(s, _selectedSearchQuery))
                .Where(s => _selectedContentPackage is null 
                            || _selectedContentPackage == ContentPackageManager.VanillaCorePackage // vanilla is treated as all packages
                            || s.OwnerPackage == _selectedContentPackage)
                .OrderBy(s => GetLocalizedString(s.GetDisplayInfo().DisplayName, s.InternalName))
                .ToImmutableArray();
        }
        
        
        bool SettingMatchesQuery(ISettingBase setting, string queryText)
        {
            if (queryText.IsNullOrWhiteSpace())
            {
                return true;
            }
            
            queryText = queryText.ToLowerInvariant().Trim();

            if (setting.InternalName.ToLowerInvariant().Trim().Contains(queryText) || setting.OwnerPackage.Name.ToLowerInvariant().Trim().Contains(queryText))
            {
                return true;
            }

            var displayInfo = setting.GetDisplayInfo();
            return TextManager.Get(displayInfo.DisplayName).Value.ToLowerInvariant().Trim().Contains(queryText)
                || TextManager.Get(displayInfo.DisplayCategory).Value.ToLowerInvariant().Trim().Contains(queryText)
                || TextManager.Get(displayInfo.Description).Value.ToLowerInvariant().Trim().Contains(queryText)
                || TextManager.Get(displayInfo.Tooltip).Value.ToLowerInvariant().Trim().Contains(queryText);
        }

        string GetPackageName(ContentPackage package)
        {
            return package is null || package == ContentPackageManager.VanillaCorePackage ? "All" : package.Name;
        }

        ContentPackage GetCurrentSelectedPackage(ImmutableArray<ContentPackage> packages)
        {
            if (_selectedContentPackage is null)
            {
                return ContentPackageManager.VanillaCorePackage;
            }
            
            if (packages.Contains(_selectedContentPackage))
            {
                return _selectedContentPackage;
            }

            if (packages.Length > 0)
            {
                _selectedContentPackage = packages[0];
                return packages[0];
            }

            return null;
        }

        void GenerateCategoryListDisplay(GUILayoutGroup layoutGroup, ImmutableArray<ContentPackage> packagesList, 
            ImmutableArray<string> categories)
        {
            layoutGroup.ClearChildren();
            var packageSelectionList = GUIUtil.Dropdown<ContentPackage>(layoutGroup, cp => GetPackageName(cp), null,
                packagesList, GetCurrentSelectedPackage(packagesList), cp =>
                {
                    _selectedContentPackage = cp;
                    _selectedCategory = string.Empty; 
                    GenerateCategoryListDisplay(_modCategoryDisplayGroup, GetTargetPackagesList(), GetDisplayCategoriesList());
                    GenerateSettingsListDisplay(_settingsDisplayGroup, GetDisplaySettingsList());
                }, new Vector2(1f, 0.07f));
            var containerBox = new GUIListBox(new RectTransform(new Vector2(1f, 0.945f), layoutGroup.RectTransform));
            const float entryHeight = 0.122f;
            float sizeY = MathF.Max(categories.Length * entryHeight, 1f);
            var displayedCategoriesFrame = new GUIFrame(new RectTransform(new Vector2(1f, sizeY), containerBox.Content.RectTransform), style: null, color: Color.Black)
            {
                CanBeFocused = false
            };
            var displayCategoriesLayout = new GUILayoutGroup(new RectTransform(Vector2.One, displayedCategoriesFrame.RectTransform));

            foreach (var category in categories)
            {
                var btn = new GUIButton(new RectTransform(new Vector2(1f, entryHeight), displayCategoriesLayout.RectTransform), 
                    text: category, color: Color.TransparentBlack)
                {
                    CanBeFocused = true,
                    CanBeSelected = true,
                    TextColor = Color.PeachPuff,
                    HoverColor = new Color(50, 50, 50, 255),
                    HoverTextColor = Color.White,
                    SelectedColor = new Color(50, 50, 50, 255),
                    SelectedTextColor = Color.White,
                    OnPressed = () =>
                    {
                        _selectedCategory = category;
                        GenerateSettingsListDisplay(_settingsDisplayGroup, GetDisplaySettingsList());
                        return true;
                    }
                };
            }
        }

        void GenerateSettingsListDisplay(GUILayoutGroup layoutGroup, ImmutableArray<ISettingBase> settings) 
        {
            layoutGroup.ClearChildren();
            const float settingHeight = 0.0625f;
            
            var containerBox = new GUIListBox(new RectTransform(new Vector2(1f, 1f), layoutGroup.RectTransform));
            foreach (var setting in settings)
            {
                var entry = AddSettingToDisplay(
                    setting,
                    containerBox.Content.RectTransform,
                    settingHeight: settingHeight,
                    labelSize: new Vector2(0.6f, 1f),
                    controlSize: new Vector2(0.4f, 1f));
                
                
            }
        }

        (GUIFrame entryFrame, GUILayoutGroup entryLayoutGroup) AddSettingToDisplay(ISettingBase setting, 
            RectTransform parent, float settingHeight, Vector2 labelSize, Vector2 controlSize)
        {
            GUIFrame entryFrame = new GUIFrame(new RectTransform(new Vector2(1f, settingHeight), parent))
            {
                Color = Color.DarkGray
            };
            GUILayoutGroup entryLayoutGroup = new GUILayoutGroup(new RectTransform(Vector2.One, entryFrame.RectTransform), isHorizontal: true);

            // padding
            new GUIFrame(new RectTransform(new Vector2(0.02f, 1f), entryLayoutGroup.RectTransform),
                color: Color.TransparentBlack);
            
            new GUITextBlock(new RectTransform(labelSize - new Vector2(0.05f, 0f), entryLayoutGroup.RectTransform),
                GetLocalizedString(setting.GetDisplayInfo().DisplayName, setting.GetDisplayInfo().DisplayName),
                textColor: Color.PeachPuff,
                font: GUIStyle.SmallFont,
                textAlignment: Alignment.Left)
            {
                ToolTip = GetLocalizedString(setting.GetDisplayInfo().Tooltip, string.Empty)
            };

            setting.AddDisplayComponent(entryLayoutGroup, controlSize, newValue =>
            {
                NewValuesCache[setting] = newValue;
            });
            return (entryFrame, entryLayoutGroup);
        }
    }


    protected override void DisposeInternal()
    {
        NewValuesCache.Clear();
        _modCategoryDisplayGroup?.Parent.RemoveChild(_modCategoryDisplayGroup);
        _settingsDisplayGroup?.Parent.RemoveChild(_settingsDisplayGroup);
        _modCategoryDisplayGroup = null;
        _settingsDisplayGroup = null;
        
    }

    public override void ApplyInstalledModChanges()
    {
        foreach (var kvp in NewValuesCache)
        {
            if (kvp.Key.IsDisposed)
            {
                continue;
            }

            kvp.Key.TrySetValue(kvp.Value);
            ConfigService.SaveConfigValue(kvp.Key);
        }
        NewValuesCache.Clear();
        OnApplyInstalledModsChanges?.Invoke();
    }
}
