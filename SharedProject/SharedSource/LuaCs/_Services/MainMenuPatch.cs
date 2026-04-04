using Barotrauma;
using Barotrauma.LuaCs;
using Barotrauma.LuaCs.Events;
using FluentResults;
using HarmonyLib;
using Microsoft.Xna.Framework;

[HarmonyPatch]
internal class MainMenuPatch : ISystem, IEventScreenSelected
{
    public bool IsDisposed { get; private set; }

    private readonly IEventService _eventService;

    private bool mainMenuUIAdded = false;

    public MainMenuPatch(IEventService eventService)
    {
        _eventService = eventService;

        RegisterEvents();

#if CLIENT
        if (Screen.Selected is MainMenuScreen mainMenuScreen)
        {
            AddToMainMenu(mainMenuScreen);
        }
#endif
    }

    public void OnScreenSelected(Screen screen)
    {
#if CLIENT
        if (screen is MainMenuScreen mainMenuScreen)
        {
            AddToMainMenu(mainMenuScreen);
        }
#endif
    }

#if CLIENT
    private void AddToMainMenu(MainMenuScreen screen)
    {
        if (mainMenuUIAdded) { return; }

        new GUITextBlock(new RectTransform(new Point(300, 30), screen.Frame.RectTransform, Anchor.TopLeft) { AbsoluteOffset = new Point(10, 10) }, $"Using LuaCsForBarotrauma revision {AssemblyInfo.GitRevision}", Color.Red)
        {
            IgnoreLayoutGroups = false
        };

        mainMenuUIAdded = true;
    }
#endif

    private void RegisterEvents()
    {
        _eventService.Subscribe<IEventScreenSelected>(this);
    }

    public void Dispose()
    {
        _eventService.Unsubscribe<IEventScreenSelected>(this);

        IsDisposed = true;
    }

    public FluentResults.Result Reset()
    {
        RegisterEvents();

        return FluentResults.Result.Ok();
    }
}
