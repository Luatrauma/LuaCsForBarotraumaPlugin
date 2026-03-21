using System;
using System.Collections.Immutable;
using Microsoft.Xna.Framework;
using System.Linq;
using Barotrauma.LuaCs.Data;

namespace Barotrauma.LuaCs;

internal sealed class ModsGameplaySettingsMenu : ModsSettingsMenu
{
    private readonly ImmutableArray<ISettingBase> _settingsInstancesGameplay;
    // menu vars
    private GUILayoutGroup _modCategoryDisplayGroup, _settingsDisplayGroup;
    private string _selectedSearchQuery = string.Empty;
    private ContentPackage _selectedContentPackage;
    private string _selectedCategory = string.Empty;
    
    public ModsGameplaySettingsMenu(GUIFrame contentFrame, 
        IPackageManagementService packageManagementService, 
        IConfigService configService, 
        SettingsMenu settingsMenuInstance) : base(contentFrame, packageManagementService, configService, settingsMenuInstance)
    {

        _settingsInstancesGameplay = configService.GetDisplayableConfigs()
            .Where(s => s is not ISettingControl)
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
            /*
            OnTextChangedDelegate = (btn, txt) =>
            {
                GenerateDisplayFromFilter(txt);
                return true;
            }
            */
        };
        // main display area
        var settingsContentAreaGroup = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.90f), contentAreaLayoutGroup.RectTransform, Anchor.BottomCenter));
        GUIUtil.Spacer(settingsContentAreaGroup, Vector2.One);
        (_modCategoryDisplayGroup, _settingsDisplayGroup) = GUIUtil.CreateSidebars(settingsContentAreaGroup, true);
        _modCategoryDisplayGroup.RectTransform.RelativeSize = new Vector2(0.3f, 1f);
        _settingsDisplayGroup.RectTransform.RelativeSize = new Vector2(0.7f, 1f);

        GenerateCategoryListDisplay(_modCategoryDisplayGroup, GetDisplayCategoriesList());
        GenerateSettingsListDisplay(_settingsDisplayGroup, GetDisplaySettingsList());
        
        void GenerateDisplayFromFilter(string text)
        {
            _selectedSearchQuery = text;
            GenerateCategoryListDisplay(_modCategoryDisplayGroup, GetDisplayCategoriesList());
            GenerateSettingsListDisplay(_settingsDisplayGroup, GetDisplaySettingsList());
        }

        ImmutableArray<string> GetDisplayCategoriesList()
        {
            return _settingsInstancesGameplay
                .Select(s => s.GetDisplayInfo().DisplayCategory)
                .Distinct()
                .ToImmutableArray();
        }

        ImmutableArray<ISettingBase> GetDisplaySettingsList()
        {
            return _settingsInstancesGameplay
                .Where(s => _selectedCategory.IsNullOrWhiteSpace() 
                            || s.GetDisplayInfo().DisplayCategory == _selectedCategory)
                .Where(s => _selectedContentPackage is null 
                            || s.OwnerPackage == _selectedContentPackage)
                .ToImmutableArray();
        }

        void GenerateCategoryListDisplay(GUILayoutGroup layoutGroup, ImmutableArray<string> categoryIdents)
        {
            layoutGroup.ClearChildren();

            var packages = _settingsInstancesGameplay.Select(s => s.OwnerPackage)
                .Distinct()
                .OrderBy(cp => cp.Name)
                .ToImmutableArray();
            var packageSelectionList = GUIUtil.Dropdown<ContentPackage>(layoutGroup, cp => cp.Name, null,
                packages, packages.Length > 0 ? packages[0] : null, cp =>
                {
                    _selectedContentPackage = cp;
                    _selectedCategory = string.Empty;
                    GenerateCategoryListDisplay(_modCategoryDisplayGroup, GetDisplayCategoriesList());
                    GenerateSettingsListDisplay(_settingsDisplayGroup, GetDisplaySettingsList());
                }, new Vector2(1f, 0.07f));
            var containerBox = new GUIListBox(new RectTransform(new Vector2(1f, 0.93f), layoutGroup.RectTransform));
            float size_y = MathF.Max(categoryIdents.Length * 0.122f, 1f);
            var displayedCategoriesFrame = new GUIFrame(new RectTransform(new Vector2(1f, size_y), containerBox.Content.RectTransform), style: null, color: Color.Black)
            {
                CanBeFocused = false
            };
            var displayCategoriesLayout = new GUILayoutGroup(new RectTransform(Vector2.One, displayedCategoriesFrame.RectTransform));

            foreach (var category in categoryIdents)
            {
                DebugConsole.Log(category);
                new GUIButton(new RectTransform(new Vector2(1f, 0.122f), displayCategoriesLayout.RectTransform), text: TextManager.Get(category))
                {
                    CanBeFocused = true,
                    CanBeSelected = true,
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
            
        }
    }


    protected override void DisposeInternal()
    {
        // TODO: Finish this later.
    }
}
